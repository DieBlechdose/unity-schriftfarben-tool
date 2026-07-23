using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editorwerkzeug zum Einfärben ausgewählter UI-Texte.
/// Unterstützt TextMeshPro sowie UnityEngine.UI.Text, ohne eine feste
/// Abhängigkeit zu einem der beiden Pakete zu erzeugen.
/// </summary>
public sealed class SchriftfarbenFenster : EditorWindow
{
    private Color schriftfarbe = Color.white;
    private bool kinderEinbeziehen = true;
    private bool inaktiveEinbeziehen = true;

    [MenuItem("Tools/Schriftfarben ändern")]
    private static void Oeffnen()
    {
        SchriftfarbenFenster fenster = GetWindow<SchriftfarbenFenster>();
        fenster.titleContent = new GUIContent("Schriftfarben");
        fenster.minSize = new Vector2(340f, 190f);
        fenster.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Schriftfarben ändern", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Wähle in der Hierarchy ein oder mehrere Objekte aus. Das Werkzeug findet " +
            "TextMeshPro- und klassische UI-Texte in der Auswahl.",
            MessageType.Info);

        EditorGUILayout.Space(4f);
        schriftfarbe = EditorGUILayout.ColorField("Neue Schriftfarbe", schriftfarbe);
        kinderEinbeziehen = EditorGUILayout.ToggleLeft(
            "Texte in untergeordneten Objekten einbeziehen", kinderEinbeziehen);

        using (new EditorGUI.DisabledScope(!kinderEinbeziehen))
        {
            inaktiveEinbeziehen = EditorGUILayout.ToggleLeft(
                "Inaktive untergeordnete Objekte einbeziehen", inaktiveEinbeziehen);
        }

        EditorGUILayout.Space(10f);
        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button("Farbe auf Auswahl anwenden", GUILayout.Height(32f)))
            {
                FarbeAnwenden();
            }
        }
    }

    private void FarbeAnwenden()
    {
        List<Component> texte = TexteInAuswahlFinden();

        if (texte.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Keine Texte gefunden",
                "In der aktuellen Auswahl wurden keine unterstützten Text-Komponenten gefunden.",
                "OK");
            return;
        }

        Undo.RecordObjects(texte.ToArray(), "Schriftfarbe ändern");

        int geaendert = 0;
        foreach (Component textKomponente in texte)
        {
            SerializedObject serializedObject = new SerializedObject(textKomponente);
            SerializedProperty farbe = IstTextMeshPro(textKomponente)
                ? serializedObject.FindProperty("m_fontColor32")
                : serializedObject.FindProperty("m_Color");

            if (farbe == null || farbe.propertyType != SerializedPropertyType.Color)
            {
                continue;
            }

            farbe.colorValue = schriftfarbe;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(textKomponente);
            geaendert++;
        }

        SceneView.RepaintAll();
        EditorUtility.DisplayDialog(
            "Fertig",
            geaendert == 1
                ? "Die Schriftfarbe eines Textes wurde geändert."
                : $"Die Schriftfarbe von {geaendert} Texten wurde geändert.",
            "OK");
    }

    private List<Component> TexteInAuswahlFinden()
    {
        var ergebnis = new List<Component>();
        var bereitsGefunden = new HashSet<int>();

        foreach (GameObject objekt in Selection.gameObjects)
        {
            Component[] komponenten = kinderEinbeziehen
                ? objekt.GetComponentsInChildren<Component>(inaktiveEinbeziehen)
                : objekt.GetComponents<Component>();

            foreach (Component komponente in komponenten)
            {
                if (komponente == null || !IstUnterstuetzterText(komponente))
                {
                    continue;
                }

                if (bereitsGefunden.Add(komponente.GetInstanceID()))
                {
                    ergebnis.Add(komponente);
                }
            }
        }

        return ergebnis;
    }

    private static bool IstUnterstuetzterText(Component komponente)
    {
        string typname = komponente.GetType().FullName;
        return typname == "UnityEngine.UI.Text"
            || typname == "TMPro.TextMeshProUGUI"
            || typname == "TMPro.TextMeshPro";
    }

    private static bool IstTextMeshPro(Component komponente)
    {
        string typname = komponente.GetType().FullName;
        return typname == "TMPro.TextMeshProUGUI"
            || typname == "TMPro.TextMeshPro";
    }
}
