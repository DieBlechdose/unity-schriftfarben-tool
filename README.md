# Unity-Schriftfarben-Tool

Mit diesem kleinen Editorwerkzeug kannst du die Farbe mehrerer Texte auf einmal
ändern. Unterstützt werden:

- TextMeshPro UI (`TextMeshProUGUI`)
- TextMeshPro 3D (`TextMeshPro`)
- klassischer Unity-UI-Text (`UnityEngine.UI.Text`)

## Installation

1. Kopiere den Ordner `Assets` aus diesem Paket in dein Unity-Projekt.
2. Warte, bis Unity das Skript kompiliert hat.
3. Öffne oben im Unity-Menü **Tools > Schriftfarben ändern**.

## Verwendung

1. Wähle in der **Hierarchy** ein oder mehrere Objekte aus.
2. Stelle im Fenster die gewünschte Farbe ein.
3. Lege fest, ob untergeordnete und inaktive Objekte berücksichtigt werden.
4. Klicke auf **Farbe auf Auswahl anwenden**.

Die Änderung lässt sich wie gewohnt mit **Strg+Z** beziehungsweise **Cmd+Z**
rückgängig machen.

## Voraussetzung

Das Werkzeug ist für Unity 2020.3 oder neuer vorgesehen. TextMeshPro muss nur
installiert sein, wenn dein Projekt TextMeshPro-Komponenten verwendet.
