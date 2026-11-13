# Как загрузить Ztoll на GitHub и получить готовые EXE

## 1. Создайте репозиторий
1. Зайдите на https://github.com/new
2. Имя: `Ztoll`, видимость любая (Public или Private), **не** ставьте галочки «Add README / .gitignore / license» (они уже есть в проекте).
3. Нажмите **Create repository**.

## 2. Загрузите файлы (через терминал)
Установите Git (https://git-scm.com), распакуйте архив проекта и в папке проекта выполните:

```powershell
git init
git add .
git commit -m "Ztoll stage 1"
git branch -M main
git remote add origin https://github.com/ВАШ_ЛОГИН/Ztoll.git
git push -u origin main
```

Без терминала: на странице пустого репозитория нажмите **uploading an existing file**,
перетащите *содержимое* папки проекта (включая скрытую папку `.github`) и нажмите **Commit changes**.
Важно: папка `.github/workflows` должна попасть в репозиторий — без неё сборка не запустится.

## 3. Дождитесь сборки
1. Вкладка **Actions** → запуск **Build Ztoll** (если Actions отключены — нажмите «I understand my workflows, enable them»).
2. Зелёная галочка = всё собралось. Красный крест = откройте запуск, затем упавший шаг,
   **скопируйте текст ошибки и пришлите мне** — я исправлю.
3. В конце страницы запуска раздел **Artifacts**:
   * **Ztoll-Setup** — внутри `Ztoll Setup.exe` (установщик)
   * **Ztoll-exe** — внутри `Ztoll.exe` (портативная версия)

## 4. Релиз с постоянной ссылкой (по желанию)
```powershell
git tag v0.1.0
git push origin v0.1.0
```
Через несколько минут файлы появятся в **Releases**.

## 5. Если Actions пишет «permission denied» при создании релиза
Settings → Actions → General → Workflow permissions → **Read and write permissions** → Save.
