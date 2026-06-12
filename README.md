# AlphaZero
현재 진행 중 잠시 홀딩 상태 (TPS장르 공부 이후 다시 진행 예정)  
## [ 프로젝트 개요 ]

## [ 데모 영상 ]
Youtube

https://youtu.be/dhDCHlfnf6w?si=EHgt609vKBqzNz9j

## [ 개발 환경 ]
- Unity 6
- c#

## [ 주요 기능 ]


## [ 아키텍처 ]
### 객체별 내부 역할 구조 설계
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



## [ 도전과 고민 ]

