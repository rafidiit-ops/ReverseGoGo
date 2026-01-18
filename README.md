# GoGo VR Interaction Techniques

This Unity VR project implements **two interaction techniques** for grabbing distant objects in virtual reality:

## 1. ReverseGoGo (Novel Technique)
**Pull objects toward you by retracting your hand**
- Extend hand beyond threshold → enables pulling
- Retract hand with Grip pressed → object flies to you exponentially
- Auto-switches to direct manipulation when object reaches you

## 2. Traditional GoGo (Poupyrev et al., 1996)
**Reach distant objects by extending your hand**
- Extend hand beyond threshold → virtual hand extends exponentially farther
- Press Grip when virtual hand reaches object → grab it
- Object follows extended virtual hand position

## Key Differences

| Feature | Traditional GoGo | ReverseGoGo |
|---------|-----------------|-------------|
| Hand Motion | Extend forward | Retract backward |
| Object Movement | Virtual hand reaches object | Object comes to you |
| Metaphor | Telescoping arm | Magnetic pull |

## Getting Started
- **Traditional GoGo Setup**: See [TRADITIONAL_GOGO_SETUP.md](Assets/TRADITIONAL_GOGO_SETUP.md)
- **ReverseGoGo Setup**: See [USER_STUDY_SETUP.md](Assets/USER_STUDY_SETUP.md)
- **Toggle Between Both**: Use `GoGoModeToggle` component

## Platform
- Unity 6 with XR Interaction Toolkit 3.2.2
- Meta Quest 2 (OpenXR)

This is a VR interaction research project!
