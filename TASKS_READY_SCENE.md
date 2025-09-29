# Задачи для доработки ReadySceneBuilder

1. Обновить `Assets/Editor/ReadySceneBuilder.cs`, чтобы он использовал тот же символ препроцессора `INPUT_SYSTEM_ENABLED`, что и рантайм-скрипты, при подключении кода новой Input System.
2. Переместить директиву `using UnityEngine.InputSystem;` и реализацию, зависящую от новой Input System, внутрь блока `#if INPUT_SYSTEM_ENABLED`, оставив fallback-меню в ветке `#else`.
3. После удаления пакета Input System убедиться, что сборка редакторских скриптов проходит без ошибок и остаётся только fallback-пункт меню.
