# Crimson Skies Launcher
Специальный лаунчер для игры Crimson Skies который фиксит русские шрифты в игре

# <a name="run"></a> Как запустить Crimson Skies на современном компьютере
Лучший способ запуска под любое разрешение и без глюков описан [здесь](http://www.wsgf.org/dr/crimson-skies/en) в Solution 1, отлично работает на переводе от 8бит

Если у вас ошибка 0xc0000022 то способ её решения описан [здесь](https://support.gog.com/hc/ru/articles/115003398269) 

# Как использовать лаунчер
1. [Выполнить инструкцию "Как запустить Crimson Skies на современном компьютере"](#run)
3. [Скачать лаунчер](https://github.com/DaniilSokolyuk/CrimsonSkiesLauncher/releases/download/1/Launcher.zip)
4. Поместить папку Launcher в папку с игрой, 
5. Запустить CrimsonLauncher.exe в папке Launcher (например C:\Program Files (x86)\Crimson Skies\Launcher\CrimsonLauncher.exe)

# Как это работает
Выполняется внедрение специального кода в процесс игры, код выполняет перехват системной функции [CreateFontIndirectA](https://docs.microsoft.com/en-us/windows/desktop/api/wingdi/nf-wingdi-createfontindirecta) и изменяет параметр lfCharSet для поддержки русского языка

# Антивирус
Некоторые антивирусы могут в момент запуска лаунчера сообщить об угрозе, это связано с тем что лаунчер использует техники DLL Inject и перехват системных функций, так как лаунчер не имеет цифровой подписи это может для антивирусов быть подозрительным.

Отчет проверки https://www.virustotal.com/ru/file/3d13c373154c4bf553e4fd0294238bbea48f7fae96d30af94f1e5dcd23f63370/analysis/1546521809/
