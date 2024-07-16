# Crimson Skies Launcher
Специальный лаунчер для игры Crimson Skies который фиксит русские шрифты в игре

# <a name="run"></a> Как запустить Crimson Skies на современном компьютере
1. Качаем и распаковываем [dgVoodoo](https://dege.freeweb.hu/dgVoodoo2/dgVoodoo2/)
2. Запустите dgVoodooCpl.exe, перейдите на вкладку DirectX снимите флажок «dgVoodoo Watermark», установите «Texture Filtering» - «Force anisotropic 16x» и «Antiasliasing (MSAA)» - "8x" и нажимаем Apply.
3. Копируйте файлы DDraw.dll и D3DImm.dll из папки dgVoodoo2\MS\x86 в папку с игрой (например C:\Program Files (x86)\Crimson Skies)
4. Переходим на сайт [PCGamingWiki](https://community.pcgamingwiki.com/files/file/1425-crimson-skies-fix-for-widescreen-ultrawide-and-multi-mon/) и нажимаем Download this file, в окне выбираем архив v2_Crimson_Skies_dgV.7z !!!
5. В архиве выбираем папку с нужынм нам расширением экрана и копируем из этой папки файл CRIMSON.EXE в папку с игрой (например C:\Program Files (x86)\Crimson Skies)

Если у вас ошибка 0xc0000022 то способ её решения описан [здесь](https://support.gog.com/hc/ru/articles/115003398269) 

# Как использовать лаунчер
1. [Выполнить инструкцию "Как запустить Crimson Skies на современном компьютере"](#run)
3. [Скачать лаунчер](https://github.com/DaniilSokolyuk/CrimsonSkiesLauncher/releases/download/1.1/Launcher.zip)
4. Поместить папку Launcher в папку с игрой, 
5. Запустить CrimsonLauncher.exe в папке Launcher (например C:\Program Files (x86)\Crimson Skies\Launcher\CrimsonLauncher.exe)

* Если лаунчре не запускает игру, откройте свойства файла crimson.exe, перейдите на вкладвку совместимость и включите "Использовать разрешение экрана 640x480"

# Как это работает
Выполняется внедрение специального кода в процесс игры, код выполняет перехват системной функции [CreateFontIndirectA](https://docs.microsoft.com/en-us/windows/desktop/api/wingdi/nf-wingdi-createfontindirecta) и изменяет параметр lfCharSet для поддержки русского языка

# Антивирус
Некоторые антивирусы могут в момент запуска лаунчера сообщить об угрозе, это связано с тем что лаунчер использует техники DLL Inject и перехват системных функций, так как лаунчер не имеет цифровой подписи это может для антивирусов быть подозрительным.

# Скриншот
![image](https://github.com/user-attachments/assets/7b2bd600-a7bd-4571-afe2-5692a4221e1e)
