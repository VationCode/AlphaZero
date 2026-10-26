# Unity Project Architecture & Collaboration Guide

## 1. 목표

- 코드의 소유 주체와 책임을 명확히 하여 이름으로 역할과 수정 영향 범위를 예측한다.
- 구조보다 유지보수성과 Unity 참조 안전성을 우선하며 **소유자 → 책임** 순서로 판단한다.

## 2. 최상위 분류

| 영역 | 책임 |
|---|---|
| Entity | Player, Enemy, Item처럼 독립적인 상태와 행동을 가진 게임 주체 |
| System | 특정 Entity에 귀속되지 않는 공용 게임 규칙과 범용 기능 |
| LDA | 외부 데이터의 로드, 저장, 조회 |
| Installer | Entity·System·Scene 범위의 의존성 조립 |

### 2.1 Entity 내부 구조

Entity는 내부 로직을 소유하는 `Feature`와 Unity 표현을 담당하는 `View`로 구성한다.

```text
Entity/<Entity>
├─ Feature
│  ├─ Core
│  ├─ Locomotion
│  │  └─ LEA 기반
│  └─ <Feature>
└─ View
```

| Entity 내부 영역 | 책임 |
|---|---|
| Feature | Entity가 소유한 상태와 행동을 기능별로 구성 |
| Core | Entity 내부 Feature 조립과 외부 연결 |
| View | Scene, UI, Animation, Effect 등 Unity 표현 |

- 특정 Entity의 월드 표현은 해당 View에 둔다. 예: `PlayerAnimationView → Entity/Player/View`.
- UI는 독립적인 Entity로 구성한다. 예: `InventoryView → Entity/UI/View/Inventory`.
- UI가 표시하는 게임 상태는 해당 게임 Entity가 소유하며 View는 전달받은 상태를 표현한다.
- 단순 표시는 View가 담당하고 게임 상태나 규칙 판단이 포함될 때만 Flow로 분리한다.

### 2.2 LEA 역할

LEA는 최상위 폴더가 아니라 Entity의 Feature 내부 클래스에 적용되는 역할 기준이다.

| LEA 역할 | 책임 |
|---|---|
| CompositionRoot / Core | Entity 내부 조립, 초기화, 외부 진입점 |
| Domain | 상태와 핵심 개념 |
| Flow / State | 실행 시점, 조건, 상태 전이, 행동 판단 |
| Module | 실제 기능 수행 |

## 3. 기본 구조

```text
Scripts
├─ Installer
├─ LDA/Loader, Database
├─ System
│  ├─ Combat, Inventory
│  └─ Utility, Extension, Base
└─ Entity/Player, Enemy, Item, UI
```

- `LEA 기반`은 실제 폴더가 아니며 `LEA`, `Domain`, `Flow`, `Module` 폴더는 만들지 않는다.
- 빈 폴더, 의미 없는 Base·Handler·Module을 구조에 맞추기 위해 만들지 않는다.
- Manager는 계층이 아니라 등록·조회·생명주기를 관리하는 역할이며 실제 소유 영역에 둔다.

## 4. LEA 상세 기준

### 4.1 목적과 적용 범위

- Player, Enemy, UI처럼 복잡한 Entity에 적용하되 단순한 Entity에는 전체 구조를 강제하지 않으며 모든 LEA 역할이 필수는 아니다.
- 실제 책임이 생겼을 때만 클래스를 분리하며 LEA 역할별 폴더는 추가하지 않는다.

### 4.2 Feature 중심 구성

```text
Entity/Player
├─ Feature
│  ├─ Core
│  │  └─ PlayerCore.cs
│  ├─ Locomotion
│  │  ├─ LocomotionContext.cs
│  │  ├─ GroundMoveState.cs
│  │  └─ PlayerLocomotionModule.cs
│  ├─ Combat
│  └─ Inventory
└─ View
```

- `Locomotion`, `Combat`, `Inventory`처럼 목적이 드러나는 이름을 사용한다.
- Feature가 커져도 LEA 역할이 아닌 실제 세부 기능을 기준으로만 추가 분리를 검토한다.
- 여러 Feature가 공유하는 Entity 공통 상태와 조립 코드는 `Feature/Core`에 둘 수 있다.

### 4.3 CompositionRoot / Core

- Domain, Flow, Module 참조를 구성한다.
- 내부 객체의 `Bind()`와 초기화 순서를 관리한다.
- Installer로부터 외부 의존성을 전달받는다.
- 외부에서 Entity를 사용할 수 있는 대표 진입점을 제공한다.
- 이동 계산, 공격 판단, 상태 전이 같은 세부 게임 로직은 직접 처리하지 않는다.

```text
Installer → PlayerCore → Player 내부 Feature
```

### 4.4 Domain

- Context, State, Data, Enum, 값 객체 등이 해당한다.
- “현재 무엇을 가지고 있고 어떤 상태인가?”에 답한다.
- 실행 순서나 Unity 표현을 직접 제어하지 않는다.
- Flow, Module, View, LDA의 구체 구현을 알지 않는다.
- 별도 Domain 폴더 없이 해당 Feature 폴더에 파일을 배치한다.

예: `PlayerContext`, `HealthState`, `WeaponDataDTO`, `ELocomotionMode`.

### 4.5 Flow / State

- 입력과 이벤트를 해석한다.
- 현재 상태와 실행 가능 조건을 확인한다.
- 상태 전이와 Module 실행 시점을 결정한다.
- 여러 Module 또는 System을 필요한 순서대로 호출한다.
- 실제 이동, 공격, 아이템 추가 같은 세부 기능은 Module에 맡긴다.
- 별도 Flow 폴더 없이 해당 Feature 폴더에 파일을 배치한다.

```text
입력 확인 → 상태 판단 → 실행 가능 여부 확인 → Module 호출
```

예: `GroundMoveState`, `CombatFlow`, `PickupFlow`, `StateMachine`.

### 4.6 Module

- “이 기능을 어떻게 실행하는가?”에 답한다.
- 이동, 회전, 공격, 장비 교체, 아이템 추가처럼 구체적인 처리를 담당한다.
- 다른 Feature의 상태 전이나 전체 행동 순서를 판단하지 않는다.
- 실행 조건과 순서 판단이 커지면 Flow 책임인지 검토한다.
- 별도 Module 폴더 없이 해당 Feature 폴더에 파일을 배치한다.

```text
PlayerLocomotionModule → MoveModule / RotationModule
PlayerCombatModule     → AttackModule / WeaponSwapModule
```

- 대표 Module은 Feature의 외부 진입점과 세부 기능 실행 순서를 담당한다.
- 세부 Module은 하나의 구체적인 기능에 집중한다.
- 외부에서는 가능한 한 대표 Module을 통해 기능을 사용한다.

### 4.7 여러 Feature의 조정

```text
PlayerActionFlow
├─ Locomotion 상태 확인
├─ Combat 상태 확인
└─ 현재 실행할 행동 결정
```

- Feature 간 조정을 Core나 개별 Module에 넣지 않는다.
- Player 전체 행동이면 Player Flow가 소유하며 공용 System으로 이동하지 않는다.
- 특정 Entity에 귀속할 수 없는 공용 규칙일 때만 System을 사용한다.

### 4.8 분리와 배치 판단

1. Entity 내부 조립과 외부 연결인가? → `CompositionRoot / Core` 역할
2. 상태와 핵심 개념인가? → `Domain` 역할
3. 실행 시점, 조건, 상태 전이를 판단하는가? → `Flow / State` 역할
4. 구체적인 기능을 수행하는가? → `Module` 역할
5. Scene, UI, Animation을 표현하는가? → `View`

- 클래스가 작다는 이유만으로 합치거나 구조를 맞추기 위해 미리 분리하지 않는다.
- 책임이 두 개 이상이고 각각 독립적으로 변경될 때 분리를 검토한다.
- 기존 Scene과 Prefab이 참조하는 `MonoBehaviour`는 가능하면 대표 진입점으로 유지한다.

## 5. 소유권과 책임

- Player의 이동·공격·습득처럼 주체가 명확한 행동은 해당 Entity Feature가 소유한다.
- 외부 객체와 협력하거나 여러 곳에서 사용된다는 이유만으로 System으로 옮기지 않는다.
- Damage, Inventory Transfer처럼 단일 Entity에 귀속하기 어려운 규칙은 System이 소유한다.
- ObjectPool, Extension, Utility 같은 Entity 독립 범용 기능도 System에 둔다.
- DTO는 이름이 아니라 표현하는 개념의 소유권으로 배치한다.
- Loader와 Database는 LDA가 소유하고 Domain은 LDA를 알지 않는다.
- Interface는 전역에 모으지 않고 계약을 소유한 기능 옆에 둔다. 예: `IDataLoader → LDA/Loader`, `IDamageable → System/Combat`.

### 5.1 독립 객체 연결 원칙

- 각 Entity와 기능 객체는 자신의 상태와 내부 실행을 소유한다.
- 외부는 대표 진입점에 명령하며 내부 Flow·Module을 직접 참조하지 않는다.
- 명령은 메서드로 전달하고 실행 결과는 이벤트나 읽기 전용 값으로 알린다.
- 객체는 상대의 구체 클래스 대신 작은 데이터나 계약을 통해 연결한다.
- Player는 입력·행동 조건·신체 반응을, Weapon은 공격 방식·내부 상태·작동 결과를 소유한다.
- 세부 종류의 차이가 설정뿐이라면 클래스를 추가하지 않고 데이터로 구성한다.

```text
외부 Flow → 대표 객체 → 내부 Flow / Module → 결과 이벤트 → 반응 객체 / View
```

## 6. 참조 및 실행 방향

```text
Input / Event → Flow / State → 대표 Module → 세부 Module → Domain → View
```

- Core는 연결하고, Flow는 판단하고, Module은 실행하며, Domain은 상태를 가지고 외부 역할을 알지 않는다.
- Module은 Domain을 다루지만 View를 직접 조작하지 않는다.
- View는 상태를 직접 변경하지 않고 Flow 또는 대표 Module에 요청한다.
- Installer는 전체 관계와 초기화 순서를 연결하며 게임 로직을 실행하지 않는다.
- 대표 Module은 Feature 진입점과 실행 순서를, 세부 Module은 하나의 기능을 담당한다.

## 7. 작업 진행 규칙

- `코드 제시`: 현재 단계에 필요한 코드만 제시한다.
- `자동 편집해줘`: 파일을 직접 수정하고 가능한 검증을 수행한다.
- 프로젝트 작업에는 `computer-use`를 사용하지 않는다.
- Unity 조작이 필요하더라도 파일·로그·CLI 기반으로 처리하며, 불가능하면 사용자에게 직접 조작을 요청한다.
- `시니어 멘토방식 진행`: AI는 코드를 바로 생성하지 않고 시니어 코드 리뷰어이자 설계 멘토로 협업한다.
  - 현재 코드와 구조를 먼저 확인한다.
  - 코드의 소유 주체와 LEA 역할을 판단한다.
  - 문제의 원인, 수정 영향 범위, 설계 선택 이유를 설명한다.
  - 가장 적합한 방향을 우선 제안하고 필요한 경우에만 대안을 비교한다.
  - 과도한 추상화보다 현재 프로젝트에 필요한 최소 구조를 권장한다.
  - 사용자가 직접 구현할 수 있도록 실행 흐름과 작업 단위를 안내한다.
  - 사용자가 작성한 코드는 책임, 결합도, 변경 영향을 기준으로 리뷰한다.
