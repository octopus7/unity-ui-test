# Battle List UI Mockup

## Lobby Layout
Full Screen Layer

```text
+---------------------------------------------------------------+
|  [My Gold: 1,500 G]          [ REFRESH ]   [ CREATE BATTLE ]  |
+---------------------------------------------------------------+
|                                                               |
|  [ SCROLL VIEW AREA START ]                                   |
|                                                               |
|  +---------------------------------------------------------+  |
|  | [ICON: MyBattle] [Battle #1024] Host: DragonSlayer99    |  |
|  | Boss HP: [//////////          ] 500/1000                |  |
|  | Attempts: 3 / 5                                         |  |
|  |                                      [ PARTICIPATE ]    |  |
|  +---------------------------------------------------------+  |
|                                                               |
|  +---------------------------------------------------------+  |
|  | [Battle #1025] Host: NewbieUser           [Time: 59m]   |  |
|  | Boss HP: [////////////////////] 1000/1000               |  |
|  | Attempts: 0 / 5                                         |  |
|  |                                      [ PARTICIPATE ]    |  |
|  +---------------------------------------------------------+  |
|                                                               |
|  +---------------------------------------------------------+  |
|  | [Battle #1020] Host: AlmostDone           [Time: 05m]   |  |
|  | Boss HP: [/                   ] 50/1000                 |  |
|  | Attempts: 4 / 5  (Last Chance!)                         |  |
|  |                                      [ PARTICIPATE ]    |  |
|  +---------------------------------------------------------+  |
|                                                               |
|  [ SCROLL VIEW AREA END ]                                     |
+---------------------------------------------------------------+
```

## Logic Notes
- **Create Battle**: Sends `POST /battle/create`. Refreshes list.
- **Participate**: Sends `POST /battle/attack`.
    - If success (Victory/Participation Prize/Damage), shows result popup.
    - If fail (Network error), shows error toast.
