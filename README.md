# AlphaZero
## 객체 내부 역할 구조
##### RootComposition(Core) : 최상위/조립
    Boundary : 외부 이벤트 입력(전달)/출력(수행)
    Flow : 흐름 제어
    Module : 기능 연산, 수행은 플젝 작을 경우 같이해도 무방
#### EX)
##### Player
    Boundary
        AnimationBoundary
    Flow
        StateFlow
    Module
        LocomotionModule
        
