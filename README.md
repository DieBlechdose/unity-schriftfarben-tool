# Unity-Schriftfarben-Tool

Mit diesem kleinen Editorwerkzeug kannst du die Farbe mehrerer Texte auf einmal
ändern. Unterstützt werden:

- TextMeshPro UI (`TextMeshProUGUI`)
- TextMeshPro 3D (`TextMeshPro`)
- klassischer Unity-UI-Text (`UnityEngine.UI.Text`)

## Installation über VRChat Creator Companion

[Schriftfarben Tool zu Creator Companion hinzufügen](https://dieblechdose.github.io/unity-schriftfarben-tool/)

Falls der Link nicht automatisch geöffnet wird, füge diese Adresse in Creator
Companion unter **Settings > Packages > Add Repository** ein:

`https://dieblechdose.github.io/unity-schriftfarben-tool/index.json`

Öffne anschließend dein Projekt in Creator Companion und füge das Paket
**Schriftfarben Tool** hinzu.

## Installation

1. Kopiere den Ordner `Assets` aus diesem Paket in dein Unity-Projekt.
2. Warte, bis Unity das Skript kompiliert hat.
3. Öffne oben im Unity-Menü **Tools > Schriftfarben ändern**.

## Verwendung

1. Wähle in der **Hierarchy** ein oder mehrere Objekte aus.
2. Stelle im Fenster die gewünschte Farbe ein.
3. Klicke auf **Farbe anwenden**.

Das Werkzeug durchsucht automatisch die gesamte Auswahl einschließlich aller
untergeordneten und inaktiven Objekte. Der Status im Fenster zeigt jederzeit,
wie viele unterstützte Textkomponenten gefunden wurden.

Die Änderung lässt sich wie gewohnt mit **Strg+Z** beziehungsweise **Cmd+Z**
rückgängig machen.

## Voraussetzung

Das Werkzeug ist für Unity 2022.3 oder neuer vorgesehen. TextMeshPro und
Unity UI werden als Paketabhängigkeiten automatisch eingebunden.
