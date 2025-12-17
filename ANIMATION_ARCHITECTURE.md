# 🎬 Архитектура анимаций в TicTacRog

## 📋 Оглавление
1. [Проблема](#проблема)
2. [Принцип разделения](#принцип-разделения)
3. [Архитектурное решение](#архитектурное-решение)
4. [Компоненты системы](#компоненты-системы)
5. [Интеграция](#интеграция)

---

## 🎯 Проблема

**Текущая ситуация (без анимаций):**
- Домен работает синхронно (мгновенно)
- UI обновляется мгновенно
- Бот ходит сразу после игрока
- Нет анимаций, нет блокировки UI

**Что нужно:**
- Игрок делает клик → анимация → UI блокирован
- Бот "думает" → делает ход → анимация → UI блокирован
- Когда анимация закончилась → UI доступен для следующего хода

---

## 🏗️ Принцип разделения

### Ключевой паттерн: Event Sourcing + Async Presentation

```
┌─────────────────────────────────────────────────┐
│         DOMAIN LAYER (синхронный)               │
│  • Просчитывает логику МГНОВЕННО                │
│  • Не знает об анимациях                        │
│  • Генерирует события                           │
│  • Хранит "логическое состояние"                │
└──────────────────┬──────────────────────────────┘
                   │ События (MoveMadeMessage)
                   ↓
┌─────────────────────────────────────────────────┐
│      PRESENTATION LAYER (асинхронный)           │
│  • Проигрывает события с ЗАДЕРЖКАМИ             │
│  • Управляет блокировкой UI                     │
│  • Очередь/последовательность анимаций          │
│  • Хранит "визуальное состояние"                │
└─────────────────────────────────────────────────┘
```

**Важно:**
- **Логическое состояние** (в домене) всегда актуальное
- **Визуальное состояние** (в UI) "догоняет" логическое через анимации
- Домен не ждет UI, события накапливаются в очереди

---

## 🏆 Архитектурное решение

### State Machine + Animation Queue + DOTween

**Профессиональное enterprise-решение**

**Суть:**
- **GameFlowStateMachine** управляет состояниями UI (WaitingForPlayer, AnimatingPlayerMove, BotThinking, AnimatingBotMove, GameFinished)
- **AnimationQueue** проигрывает события последовательно
- **DOTween** создает плавные анимации
- **VContainer** связывает всё через DI

**Файлы:**
- `GameFlowStateMachine.cs` - управление состояниями
- `AnimationQueue.cs` - очередь анимаций
- `MoveAnimationEvent.cs` - событие хода
- `GamePresenter.cs` - презентер с State Machine
- `CellView.cs` - вьюха с DOTween анимациями
- `GameLifetimeScope.cs` - DI контейнер

**Диаграмма:**

```
Игрок кликает
    ↓
MakeMoveUseCase.Execute() (мгновенно)
    ↓
MoveMadeMessage → MessageBus
    ↓
GamePresenter получает событие
    ↓
Создает MoveAnimationEvent → добавляет в AnimationQueue
    ↓
AnimationQueue.PlayQueueCoroutine():
    - Блокирует UI (через State Machine)
    - Проигрывает DOTween анимацию
    - OnQueueCompleted
    ↓
GameFlowStateMachine → BotThinking
    ↓
Задержка 0.5сек (бот "думает")
    ↓
BotPlayer.TryMakeMove() (мгновенно)
    ↓
MoveMadeMessage → MessageBus
    ↓
... цикл повторяется
```

**Плюсы:**
- ✅ Максимальная ясность состояний
- ✅ Правильное разделение логики и визуализации
- ✅ Масштабируется (можно добавлять любые события)
- ✅ Легко дебажить (логи переходов State Machine)
- ✅ Красивые анимации (DOTween)
- ✅ Профессиональный уровень (AAA студии)

---

## 🧩 Компоненты системы

### 1. GameFlowStateMachine

```csharp
public enum GameFlowState
{
    WaitingForPlayerInput,   // UI разблокирован
    AnimatingPlayerMove,     // UI заблокирован
    BotThinking,             // UI заблокирован
    AnimatingBotMove,        // UI заблокирован
    GameFinished             // UI заблокирован
}

public class GameFlowStateMachine
{
    public event Action<GameFlowState, GameFlowState> OnStateChanged;
    
    private void TransitionTo(GameFlowState newState)
    {
        Debug.Log($"[SM] {_currentState} → {newState}");
        _currentState = newState;
        OnStateChanged?.Invoke(newState, _previousState);
    }
}
```

**Граф переходов:**

```
WaitingForPlayer → AnimatingPlayerMove → BotThinking → AnimatingBotMove → WaitingForPlayer
                                                                             │
                                                                             ↓
                                                                        GameFinished
```

### 2. AnimationQueue

```csharp
public class AnimationQueue : MonoBehaviour
{
    private Queue<IAnimationEvent> _queue = new();
    
    public void Enqueue(IAnimationEvent evt)
    {
        _queue.Enqueue(evt);
        if (!_isPlaying) StartCoroutine(PlayQueueCoroutine());
    }
    
    private IEnumerator PlayQueueCoroutine()
    {
        _isPlaying = true;
        
        while (_queue.Count > 0)
        {
            var evt = _queue.Dequeue();
            OnEventStarted?.Invoke(evt);
            yield return evt.PlayAnimation();
            OnEventCompleted?.Invoke(evt);
        }
        
        _isPlaying = false;
        OnQueueCompleted?.Invoke();
    }
}
```

**Паттерн:** Command Queue

### 3. CellView с DOTween

```csharp
public class CellView : MonoBehaviour, IAnimatable
{
    public IEnumerator PlayAnimation()
    {
        // DOTween анимация
        var sequence = DOTween.Sequence();
        
        // 1. Появление с масштабом
        transform.localScale = Vector3.zero;
        sequence.Append(
            transform.DOScale(1.15f, 0.24f).SetEase(Ease.OutBack)
        );
        
        // 2. Отскок
        sequence.Append(
            transform.DOScale(1f, 0.16f).SetEase(Ease.OutQuad)
        );
        
        // 3. Fade текста
        sequence.Join(
            _label.DOFade(1f, 0.2f).SetEase(Ease.OutQuad)
        );
        
        yield return sequence.WaitForCompletion();
    }
}
```

### 4. GamePresenter

```csharp
public class GamePresenter
{
    private readonly GameFlowStateMachine _stateMachine;
    private readonly AnimationQueue _animationQueue;
    
    private void OnCellClicked(CellIndex index)
    {
        // Проверка через State Machine
        if (!_stateMachine.CanPlayerMove()) return;
        
        // Домен работает мгновенно
        _makeMove.Execute(index);
        
        // Уведомляем State Machine
        _stateMachine.OnPlayerMoved();
    }
    
    private void OnMoveMade(MoveMadeMessage msg)
    {
        // Создаем событие анимации
        var animEvent = new MoveAnimationEvent(...);
        
        // Добавляем в очередь (проиграется автоматически)
        _animationQueue.Enqueue(animEvent);
    }
    
    private void OnStateChanged(GameFlowState newState, GameFlowState oldState)
    {
        // Блокируем/разблокируем UI в зависимости от состояния
        bool canInteract = newState == GameFlowState.WaitingForPlayerInput;
        SetInteractionEnabled(canInteract);
        
        // Обновляем статус
        UpdateStatusText(newState);
    }
}
```

### 5. GameLifetimeScope (DI)

```csharp
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private BoardView _boardView;
    [SerializeField] private StatusView _statusView;
    [SerializeField] private AnimationQueue _animationQueue;
    [SerializeField] private float _botThinkDelay = 0.5f;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // Views из сцены
        builder.RegisterInstance(_boardView);
        builder.RegisterInstance(_statusView);
        builder.RegisterInstance(_animationQueue);
        
        // Infrastructure
        builder.Register<MessageBus>(Lifetime.Singleton).As<IMessageBus>();
        builder.Register<InMemoryBoardRepository>(Lifetime.Singleton).As<IBoardRepository>();
        builder.Register<GameEventsAdapter>(Lifetime.Singleton).As<IGameEvents>();
        
        // Domain
        builder.Register<Classic3x3RuleSet>(Lifetime.Singleton).As<IGameRuleSet>();
        builder.Register<RandomBotPlayer>(Lifetime.Singleton).As<IBotPlayer>();
        
        // Use Cases
        builder.Register<StartNewGameUseCase>(Lifetime.Singleton);
        builder.Register<MakeMoveUseCase>(Lifetime.Singleton);
        
        // State Machine
        builder.Register<GameFlowStateMachine>(Lifetime.Singleton)
            .WithParameter("botThinkDelay", _botThinkDelay);
        
        // Presenter
        builder.Register<GamePresenter>(Lifetime.Singleton);
        
        // Entry Point
        builder.RegisterEntryPoint<GameEntryPoint>();
    }
}
```

---

## 🎯 Полный поток данных

```
1. [UI] Игрок кликает CellView
   ↓
2. [Presenter] GamePresenter.OnCellClicked(index)
   • _stateMachine.CanPlayerMove()? → true
   • _makeMove.Execute(index)
   • _stateMachine.OnPlayerMoved()
   ↓
3. [Domain] MakeMoveUseCase.Execute() — МГНОВЕННО!
   • board.SetMark(index, Mark.Cross)
   • CheckWin()
   • SwitchPlayer()
   • _events.OnMoveMade(state, index)
   ↓
4. [Infrastructure] MessageBus.Publish(MoveMadeMessage)
   ↓
5. [Presenter] GamePresenter.OnMoveMade(msg)
   • Создает MoveAnimationEvent
   • _animationQueue.Enqueue(event)
   ↓
6. [State Machine] TransitionTo(AnimatingPlayerMove)
   • OnStateChanged → SetInteractionEnabled(false)
   ↓
7. [AnimationQueue] PlayQueueCoroutine()
   • OnEventStarted → блокировка
   • yield return event.PlayAnimation() — 0.4сек DOTween
   • OnEventCompleted
   • OnQueueCompleted
   ↓
8. [State Machine] OnAnimationCompleted()
   • Анализирует состояние
   • TransitionTo(BotThinking)
   ↓
9. [State Machine] BotThinkCoroutine()
   • yield return WaitForSeconds(0.5)
   • _botPlayer.TryMakeMove(state)
   • TransitionTo(AnimatingBotMove)
   ↓
10. Цикл повторяется с шага 3 (бот сделал ход)
```

---

## 🔧 Интеграция

### Смотри подробный гайд: `VCONTAINER_INTEGRATION.md`

**Краткий чеклист:**

1. ✅ Установить VContainer
2. ✅ Создать UI (Canvas, BoardView, StatusView)
3. ✅ Создать Cell Prefab (с CellView, DOTween анимациями)
4. ✅ Создать AnimationQueue в сцене
5. ✅ Создать GameLifetimeScope в сцене
6. ✅ Подключить все зависимости в Inspector
7. ✅ Play!

---

## 📊 Диаграмма архитектуры

```
┌─────────────────────────────────────────────────────────────────┐
│                         PRESENTATION                            │
│                                                                 │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐     │
│  │   BoardView  │    │  StatusView  │    │ AnimationQueue│     │
│  └──────────────┘    └──────────────┘    └──────────────┘     │
│         │                    │                    │             │
│         └────────────────────┴────────────────────┘             │
│                              │                                  │
│                    ┌─────────▼────────┐                        │
│                    │  GamePresenter   │                        │
│                    └─────────┬────────┘                        │
│                              │                                  │
│              ┌───────────────┴───────────────┐                │
│              │                               │                │
│    ┌─────────▼────────┐         ┌──────────▼────────┐        │
│    │ GameFlowStateMachine│       │  AnimationQueue   │        │
│    │  (State управление) │       │  (Command Queue)  │        │
│    └─────────┬────────┘         └──────────┬────────┘        │
│              │                               │                │
└──────────────┼───────────────────────────────┼────────────────┘
               │ Events                        │ Animations
┌──────────────▼───────────────────────────────▼────────────────┐
│                      INFRASTRUCTURE                            │
│                                                                 │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐     │
│  │  MessageBus  │    │ GameEventsAdap│    │  Repository  │     │
│  └──────┬───────┘    └──────┬───────┘    └──────┬───────┘     │
│         │                    │                    │             │
└─────────┼────────────────────┼────────────────────┼─────────────┘
          │                    │                    │
┌─────────▼────────────────────▼────────────────────▼─────────────┐
│                           CORE                                   │
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐      │
│  │    Board     │    │  GameState   │    │ IGameRuleSet │      │
│  └──────────────┘    └──────────────┘    └──────────────┘      │
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐      │
│  │ MakeMoveUseCase│  │StartNewGameUC │    │  BotPlayer   │      │
│  └──────────────┘    └──────────────┘    └──────────────┘      │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘

                        VContainer (DI)
                 автоматически связывает всё
```

---

## 🎓 Ключевые принципы

### 1. Разделяй логику и визуализацию

```csharp
// ❌ ПЛОХО: домен ждет UI
public void MakeMove(CellIndex index)
{
    board.SetMark(index, player);
    await AnimateMove(); // ❌ домен зависит от UI
    CheckWin();
}

// ✅ ХОРОШО: домен мгновенный
public void MakeMove(CellIndex index)
{
    board.SetMark(index, player);
    CheckWin();
    _events.OnMoveMade(state, index); // UI сам анимирует
}
```

### 2. События — асинхронные, домен — синхронный

**Домен** (мгновенно):
- board.SetMark() — T=0мс
- CheckWin() — T=0мс
- SwitchPlayer() — T=0мс
- Publish(Event) — T=0мс

**UI** (с задержками):
- Enqueue(Event) — T=0мс
- PlayAnimation() — T=0-400мс
- OnQueueCompleted — T=400мс

### 3. UI блокируется, домен — нет

```csharp
// ✅ Правильно
if (_animationQueue.IsPlaying) return; // блокируем ввод
_makeMove.Execute(index); // домен не блокируется
```

### 4. Бот реагирует на State Machine

```csharp
// State Machine управляет ходами бота
private void OnAnimationCompleted()
{
    if (state.CurrentPlayer == Mark.Nought)
    {
        TransitionTo(BotThinking);
        StartCoroutine(BotThinkCoroutine());
    }
}
```

---

## 🏆 Профессиональность

Это решение использует:

1. **Clean Architecture** — слои приложения
2. **State Machine Pattern** — управление состояниями
3. **Command Queue Pattern** — очередь анимаций
4. **Observer Pattern** — события
5. **Dependency Injection** — VContainer
6. **MVP Pattern** — Model-View-Presenter
7. **Strategy Pattern** — IGameRuleSet
8. **Repository Pattern** — IBoardRepository

**Используется в:**
- Hearthstone (Blizzard)
- XCOM (Firaxis)
- Civilization (Firaxis)
- AAA turn-based игры

---

## 🎯 Итог

> **Домен думает мгновенно, UI показывает красиво** 🎬

Это профессиональное enterprise-решение, готовое для продакшна.
