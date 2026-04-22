# AlphaZero
# 객체 설계 구조
##### RootComposition : 최상위/조립
    Boundary : 입력/출력 외부 이벤트
    Flow : 흐름 제어
    Module : 기능 수행
#### EX)
##### Player
    Boundary
        AnimationBoundary
    Flow
        StateFlow
    Module
        LocomotionModule
        
