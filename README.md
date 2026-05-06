# AlphaZero
## 객체 내부 역할 구조
##### Installer : 최상위들 연결지점 (Bind)
##### RootComposition(Core) : 전체 조립 / DI
    Boundary : 외부와 연결(입력 / 출력 전달만)
    Domain : 데이터 / 개념
    Flow: 상태 / 흐름 / 의사결정
    Module: 기능 실행
#### EX)
##### Player
    Boundary
        AnimationBoundary
    Flow
        StateFlow
    Module
        LocomotionModule
        
