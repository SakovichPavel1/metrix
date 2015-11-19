// Fill out your copyright notice in the Description page of Project Settings.

#include "DeepIntoMe.h"
#include "MainCharacter.h"
#include "DIMPlayerState.h"
#include "DeepIntoMeHUD.h"
#include "DeepIntoMePlayerController.h"
#include "DeepIntoMeGameState.h"

// Sets default values
void AMainCharacter()
{
	// Set this character to replicate
	bReplicates = true;

	// Set this character to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	GetCapsuleComponent()->InitCapsuleSize(42.f, 96.0f);

	BaseRate = 45;
	Camera = CreateDefaultSubobject<UCameraComponent>(TEXT("Main Camera"));
	FirstPersonMesh = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("First Person Mesh"));
	Camera->AttachTo(GetCapsuleComponent());
	Camera->bUsePawnControlRotation = true;
	FirstPersonMesh->AttachTo(Camera);
	Health = 100.0f;
	
	OnActorBeginOverlap.AddDynamic(this, &OnBeginOverlap);
	OnActorEndOverlap.AddDynamic(this, &OnEndOverlap);
}

// Called when the game starts or when spawned
void BeginPlay()
{
	bTestIsDead = false;
	
	FActorSpawnParameters SpawnParameters;
	SpawnParameters.SpawnCollisionHandlingOverride = ESpawnActorCollisionHandlingMethod::AdjustIfPossibleButAlwaysSpawn;
	AWeapon* NewWeapon = GetWorld()->SpawnActor<AWeapon>(WeaponType, FVector(0, 0, 1000), FRotator(0, 0, 0), SpawnParameters);

	if (NewWeapon)
	{
		AddWeapon(NewWeapon);
	}

	StopRunning();
}

bool IsMagazineEmpty()
{
	return (Weapon) ? (Weapon->GetAmmoInClipCount() == 0) : true;
}

bool CanReload()
{
	return (Weapon) ? Weapon->CanReload() : false;
}

// Called to bind functionality to input
void SetupPlayerInputComponent(class UInputComponent* InputComponent)
{
	Super::SetupPlayerInputComponent(InputComponent);

	//InputComponent->BindAction("Jump", IE_Pressed, this, &ACharacter::Jump);
	//InputComponent->BindAction("Jump", IE_Released, this, &ACharacter::StopJumping);

	InputComponent->BindAxis("LookUp", this, &LookUp);
	InputComponent->BindAxis("LookRight", this, &LookRight);
	InputComponent->BindAxis("MoveForward", this, &MoveForward);
	InputComponent->BindAxis("MoveRight", this, &MoveRight);

	InputComponent->BindAction("Use", IE_Pressed, this, &UseItem);
	//InputComponent->BindAction("Drop Item", IE_Pressed, this, &NetMulticastDropWeapon);
	InputComponent->BindAction("Fire", IE_Pressed, this, &StartFire);
	InputComponent->BindAction("Fire", IE_Released, this, &StopFire);
	InputComponent->BindAction("Reload", IE_Pressed, this, &Reload);
	InputComponent->BindAction("Aim", IE_Pressed, this, &StartAiming);
	InputComponent->BindAction("Aim", IE_Released, this, &StopAiming);
	//InputComponent->BindAction("Crouch", IE_Pressed, this, &StartCrouching);
	//InputComponent->BindAction("Crouch", IE_Released, this, &StopCrouching);
	InputComponent->BindAction("Running", IE_Pressed, this, &StartRunning);
	InputComponent->BindAction("Running", IE_Released, this, &StopRunning);
}

void LookUp(float Value)
{
	YLookRate = Value;
	AddControllerPitchInput(Value * BaseRate * GetWorld()->GetDeltaSeconds());
}

void LookRight(float Value)
{
	XLookRate = Value;
	AddControllerYawInput(Value * BaseRate * GetWorld()->GetDeltaSeconds());
}

void MoveForward(float Value)
{
	if (Value != 0.0f)
	{
		AddMovementInput(GetActorForwardVector(), Value);
	}
}

void MoveRight(float Value)
{
	if (Value != 0.0f)
	{
		AddMovementInput(GetActorRightVector(), Value);
	}
}

void StartFire()
{
	if (Weapon && !bReloading && Weapon->GetAmmoInClipCount() > 0)
	{
		bFiring = true;
		Weapon->StartFire();
	}
	
	if (Role < ROLE_Authority)
	{
		ServerStartFire();
	}
}

void ServerStartFire_Implementation()
{
	StartFire();
}

bool ServerStartFire_Validate()
{
	return true;
}

void StopFire()
{
	if (Weapon)
	{
		bFiring = false;
		Weapon->StopFire();
	}
	
	if (Role < ROLE_Authority)
	{
		ServerStopFire();
	}
}

void ServerStopFire_Implementation()
{
	StopFire();
}

bool ServerStopFire_Validate()
{
	return true;
}


void Reload()
{
	if (Weapon && CanReload())
	{
		bReloading = true;
		Weapon->Reload();
	}
	
	if (Role < ROLE_Authority)
	{
		ServerReload();
	}
}

void ServerReload_Implementation()
{
	Reload();
}

bool ServerReload_Validate()
{
	return true;
}

void StartAiming()
{
	SetAiming(true);
}

void StopAiming()
{
	SetAiming(false);
}

void SetAiming(bool Aiming)
{
	bAiming = Aiming;

	if (Role < ROLE_Authority)
	{
		ServerSetAiming(Aiming);
	}
}

void ServerSetAiming_Implementation(bool Aiming)
{
	SetAiming(Aiming);
}

bool ServerSetAiming_Validate(bool Aiming)
{
	return true;
}

void StartCrouching()
{
	SetChourching(true);
}

void StopCrouching()
{
	SetChourching(false);
}

void SetChourching(bool Crouching)
{
	bCrouching = Crouching;
	
	if (Role < ROLE_Authority)
	{
		ServerSetChourching(Crouching);
	}
}
	
void ServerSetChourching_Implementation(bool Crouching)
{
	SetChourching(Crouching);
}

bool ServerSetChourching_Validate(bool Crouching)
{
	return true;
}

void StartRunning()
{
	SetRunningStatus(true);
}

void StopRunning()
{
	SetRunningStatus(false);
}

void SetRunningStatus(bool Running)
{
	bRunning = Running;
	if (bRunning)
	{
		GetCharacterMovement()->MaxWalkSpeed = RunningSpeed;
	}
	else
	{
		GetCharacterMovement()->MaxWalkSpeed = WalkingSpeed;
	}
		
	if (Role < ROLE_Authority)
	{
		ServerSetRunningStatus(Running);
	}
}

void ServerSetRunningStatus_Implementation(bool Running)
{
	SetRunningStatus(Running);
}

bool ServerSetRunningStatus_Validate(bool Running)
{
	return true;
}

bool IsRunning()
{
	return bRunning;
}

void AddWeapon(AWeapon* NewWeapon)
{
	if (Weapon)
	{
		NetMulticastDropWeapon();
	}
	
	if (NewWeapon)
	{
		AttachWeaponToCharacter(NewWeapon);
		NewWeapon->StopDestroyTimer();
	}
}

USkeletalMeshComponent* GetWeaponMesh()
{
	return (Weapon) ? Weapon->GetWeaponMesh() : NULL;
}

AWeapon* GetWeapon()
{
	return Weapon;
}

float GetHealth()
{
	return Health;
}

void UseItem()
{
	if (Items.Num())
	{
		auto Iterator = Items.CreateIterator();
		Iterator.Value()->OnUsed(this);
	}
}

void OnBeginOverlap(AActor* OtherActor)
{
	IUsableInterface* UsableItem = Cast<IUsableInterface>(OtherActor);

	if (UsableItem)
	{
		if (GEngine)
		{
			FString Message = UsableItem->GetActionMessage();
			GEngine->AddOnScreenDebugMessage(-1, 3, FColor::Red, Message);
		}
		Items.Add(OtherActor->GetHumanReadableName(), UsableItem);
	}
}

void OnEndOverlap(AActor* OtherActor)
{
	IUsableInterface* UsableItem = Cast<IUsableInterface>(OtherActor);

	if (UsableItem)
	{
		Items.Remove(OtherActor->GetHumanReadableName());
	}
}

float TakeDamage(float DamageAmount, struct FDamageEvent const& DamageEvent, class AController* EventInstigator, AActor* DamageCauser)
{
	CheckDeath(DamageAmount, DamageEvent, EventInstigator, DamageCauser);
	
	return DamageAmount;
}

void CheckDeath(float DamageAmount, struct FDamageEvent const& DamageEvent, class AController* EventInstigator, AActor* DamageCauser)
{
	if (Role < ROLE_Authority)
	{
		ServerCheckDeath(DamageAmount, DamageEvent, EventInstigator, DamageCauser);
	}
	else
	{
		if (!bTestIsDead)
		{
			Health -= DamageAmount;

			if (Health < 0)
			{	
				ADIMPlayerState* KillerPS = NULL;
				ADIMPlayerState* VictimPS = NULL;
			
				if (EventInstigator)
				{
					KillerPS = Cast<ADIMPlayerState>(EventInstigator->PlayerState);
					if (KillerPS && KillerPS != PlayerState)
					{
						KillerPS->AddKill();
					}
				}
				else
				{
					GEngine->AddOnScreenDebugMessage(-1, 3, FColor::Yellow, PlayerState->PlayerName);
					GEngine->AddOnScreenDebugMessage(-1, 3, FColor::Red, TEXT("HAS BEEN KILLED BY UNDETECTED REASON"));
				}
				
				VictimPS = Cast<ADIMPlayerState>(PlayerState);
				if (VictimPS)
				{
					VictimPS->AddDeath();
				}
				
				// Broadcast kill info from server
				ADeepIntoMeGameState* GameState = Cast<ADeepIntoMeGameState>(GetWorld()->GetGameState());
				if (GameState)
				{
					FString KillerName = (KillerPS) ? KillerPS->PlayerName : TEXT("");
					FString VictimName = (VictimPS) ? VictimPS->PlayerName : TEXT("");
					
					GameState->BroadcastKillMessage(KillerName, VictimName);
				}
				
				OnDying();
			}
		}
	}
}

void ServerCheckDeath_Implementation(float DamageAmount, struct FDamageEvent const& DamageEvent, class AController* EventInstigator, AActor* DamageCauser)
{
	CheckDeath(DamageAmount, DamageEvent, EventInstigator, DamageCauser);
}

bool ServerCheckDeath_Validate(float DamageAmount, struct FDamageEvent const& DamageEvent, class AController* EventInstigator, AActor* DamageCauser)
{
	return true;
}

void ServerDeath_Implementation()
{
	OnDying();
}

bool ServerDeath_Validate()
{
	return true;
}

void OnDying()
{
	Health = 0;
	bTestIsDead = true;

	NetMulticastDropWeapon();
	
	SetSpectatorMode();
}

void SetSpectatorMode()
{	
	ADeepIntoMePlayerController* PlayerController = Cast<ADeepIntoMePlayerController>(Controller);
	if (PlayerController)
	{
		PlayerController->SetSpectatorMode();
	}
			
	Destroy();
}

void AttachWeaponToCharacter(AWeapon* NewWeapon)
{
	Weapon = NewWeapon;
	Weapon->SetParentCharacter(this);
	Weapon->SetSimulatePhysics(false);
	
	//add branch for owner/replicated
	//if (IsLocallyControlled())
	//{
		Weapon->AttachRootComponentTo(FirstPersonMesh, SocketName, EAttachLocation::SnapToTarget);
	//}
	//else 
	//{
		//Weapon->AttachRootComponentTo(Mesh, SocketName, EAttachLocation::SnapToTarget);
	//}
}

void DropWeapon()
{
	if (Weapon)
	{
		Weapon->StopFire();
		Weapon->DetachRootComponentFromParent(true);
		Weapon->SetParentCharacter(NULL);	
		Weapon->SetSimulatePhysics(true);
		Weapon->StartDestroyTimer();
		Weapon = NULL;
	}
}

void NetMulticastDropWeapon_Implementation()
{
	DropWeapon();
}

void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps)
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(AMainCharacter, Health);
	DOREPLIFETIME(AMainCharacter, bTestIsDead);
	
	// Firing & movement properties
	DOREPLIFETIME(AMainCharacter, bReloading);
	DOREPLIFETIME(AMainCharacter, bAiming);
	DOREPLIFETIME(AMainCharacter, bCrouching);
	DOREPLIFETIME(AMainCharacter, bRunning);
}