# 🎮 TicTacRog

**Профессиональная реализация крестиков-ноликов на Unity с Clean Architecture**

---

## 🏆 Особенности

- ✅ **Clean Architecture** — разделение на Domain, Infrastructure, Presentation
- ✅ **State Machine** — управление игровым потоком
- ✅ **Animation Queue** — последовательные анимации
- ✅ **DOTween** — плавные анимации появления
- ✅ **VContainer** — Dependency Injection
- ✅ **Event-Driven** — слабая связанность компонентов
- ✅ **Тестируемость** — 100% бизнес-логики можно тестировать
- ✅ **Масштабируемость** — легко добавлять фичи

---

## 🏗️ Архитектура

```
┌─────────────────────────────────────┐
│      PRESENTATION (UI)              │
│  • Views (BoardView, CellView)      │
│  • Presenters (GamePresenter)       │
│  • State Machine (GameFlowSM)       │
│  • BotController                    │
│  • Animation Queue                  │
└──────────────┬──────────────────────┘
               ↓ зависит от
┌──────────────▼──────────────────────┐
│   INFRASTRUCTURE (Реализации)       │
│  • MessageBus                       │
│  • Repositories                     │
│  • Event Adapters                   │
└──────────────┬──────────────────────┘
               ↓ зависит от
┌──────────────▼──────────────────────┐
│        CORE (Бизнес-логика)         │
│  • Domain (Board, GameState)        │
│  • Use Cases (MakeMove, StartGame)  │
│  • Interfaces                       │
└─────────────────────────────────────┘
```

**Зависимости идут только СВЕРХУ ВНИЗ!**

---

## 🎯 Ключевой принцип

> **Домен думает мгновенно, UI показывает красиво** 🎬

```
Игрок кликает
    ↓
Домен просчитывает ВСЁ мгновенно
    ↓
События попадают в очередь анимаций
    ↓
UI проигрывает их последовательно
    ↓
Готово!
```

---

## 📁 Структура проекта

```
Assets/TicTacRog/
├── Core/                    # Бизнес-логика
│   ├── Domain/              # Сущности
│   │   ├── Board.cs
│   │   ├── GameState.cs
│   │   ├── Mark.cs
│   │   └── IGameRuleSet.cs
│   └── UseCases/            # Сценарии
│       ├── MakeMoveUseCase.cs
│       └── StartNewGameUseCase.cs
│
├── Infrastructure/          # Реализации
│   ├── Events/
│   │   ├── MessageBus.cs
│   │   └── GameEventsAdapter.cs
│   └── Repositories/
│       └── InMemoryBoardRepository.cs
│
└── Presentation/            # UI
    ├── Views/
    │   ├── BoardView.cs
    │   ├── CellView.cs      # + DOTween анимации
    │   └── StatusView.cs
    ├── Presenters/
    │   └── GamePresenter.cs
    ├── Animation/
    │   ├── AnimationQueue.cs
    │   └── MoveAnimationEvent.cs
    ├── StateMachine/
    │   ├── GameFlowState.cs
    │   └── GameFlowStateMachine.cs
    └── DI/
        ├── GameLifetimeScope.cs    # VContainer
        └── GameEntryPoint.cs
```

---

## 🎨 Используемые паттерны

1. **Clean Architecture** — слои приложения
2. **State Machine** — управление состояниями
3. **Command Queue** — очередь анимаций
4. **Observer** — события (MessageBus)
5. **Command** — Use Cases
6. **Strategy** — IGameRuleSet
7. **Repository** — IBoardRepository
8. **MVP** — Model-View-Presenter
9. **Dependency Injection** — VContainer
10. **Event Sourcing** — события от домена

---

## 🧪 Тестируемость

Вся бизнес-логика тестируется без Unity:

```csharp
[Test]
public void TestWinCondition()
{
    // Arrange
    var board = new Board(3);
    var ruleSet = new Classic3x3RuleSet();
    
    // Act
    board.SetMark(new CellIndex(0, 0), Mark.Cross);
    board.SetMark(new CellIndex(0, 1), Mark.Cross);
    board.SetMark(new CellIndex(0, 2), Mark.Cross);
    var result = ruleSet.Evaluate(board, Mark.Cross, new CellIndex(0, 2));
    
    // Assert
    Assert.AreEqual(GameStatus.Win, result);
}
```

---

## 🔧 Настройки (в Inspector)

**GameLifetimeScope:**
- **Board Size**: 3 (размер доски)
- **Starting Player**: Cross (кто ходит первым)
- **Bot Think Delay**: 0.5 (задержка "думания" бота)

**CellView:**
- **Animation Duration**: 0.4 (длительность анимации)

---

## 🚀 Расширения

Легко добавить:
- ✅ Сетевую игру (новый репозиторий)
- ✅ Доску 5x5 (новый RuleSet)
- ✅ Разные AI (новый IBotPlayer)
- ✅ Реплеи (сохранение событий)
- ✅ Отмену ходов (Command Pattern)
- ✅ Разные темы UI

---

**Clean Architecture + State Machine + Animation Queue + DOTween + VContainer = Enterprise Solution**
