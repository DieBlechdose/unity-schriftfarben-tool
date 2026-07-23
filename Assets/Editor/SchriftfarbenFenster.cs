using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DieBlechdose.SchriftfarbenTool.Editor
{
    /// <summary>
    /// Ändert die Farbe aller unterstützten Texte in der aktuellen
    /// Hierarchy-Auswahl und deren Unterobjekten.
    /// </summary>
    public sealed class SchriftfarbenFenster : EditorWindow
    {
        private const string Menuepfad = "Tools/Schriftfarben ändern";
        private const string UndoName = "Schriftfarbe ändern";

        private readonly List<GameObject> ausgewaehlteObjekte = new List<GameObject>();
        private readonly List<Component> gefundeneTexte = new List<Component>();

        private Color schriftfarbe = Color.white;
        private string hexFarbe = "#FFFFFFFF";
        private Vector2 scrollPosition;

        [MenuItem(Menuepfad)]
        private static void Oeffnen()
        {
            SchriftfarbenFenster fenster = GetWindow<SchriftfarbenFenster>();
            fenster.titleContent = new GUIContent("Schriftfarben");
            fenster.minSize = new Vector2(360f, 300f);
            fenster.Show();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += AuswahlAktualisieren;
            AuswahlAktualisieren();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= AuswahlAktualisieren;
        }

        private void OnHierarchyChange()
        {
            AuswahlAktualisieren();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Space(12f);
            KopfbereichZeichnen();
            EditorGUILayout.Space(10f);
            StatusbereichZeichnen();
            EditorGUILayout.Space(14f);
            FarbauswahlZeichnen();
            EditorGUILayout.Space(18f);
            AnwendenButtonZeichnen();
            EditorGUILayout.Space(12f);

            EditorGUILayout.EndScrollView();
        }

        private void KopfbereichZeichnen()
        {
            EditorGUILayout.BeginVertical(Styles.Karte);
            EditorGUILayout.LabelField("Schriftfarbe ändern", Styles.Titel);
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Ändert automatisch die Farbe aller unterstützten Texte innerhalb " +
                "der aktuell ausgewählten Hierarchy-Objekte und deren Unterobjekte.",
                Styles.Beschreibung);
            EditorGUILayout.EndVertical();
        }

        private void StatusbereichZeichnen()
        {
            EditorGUILayout.BeginVertical(Styles.Karte);
            EditorGUILayout.LabelField("Status", Styles.Abschnittstitel);
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(AuswahlStatusText(), Styles.Status);
            EditorGUILayout.LabelField(TextStatusText(), Styles.Status);
            EditorGUILayout.EndVertical();
        }

        private void FarbauswahlZeichnen()
        {
            EditorGUILayout.BeginVertical(Styles.Karte);
            EditorGUILayout.LabelField("Neue Schriftfarbe", Styles.Abschnittstitel);
            EditorGUILayout.Space(6f);

            Color neueFarbe = EditorGUILayout.ColorField(
                GUIContent.none,
                schriftfarbe,
                true,
                true,
                false,
                GUILayout.Height(38f));

            if (neueFarbe != schriftfarbe)
            {
                schriftfarbe = neueFarbe;
                hexFarbe = "#" + ColorUtility.ToHtmlStringRGBA(schriftfarbe);
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hex", GUILayout.Width(32f));

            string neuerHexwert = EditorGUILayout.TextField(hexFarbe);
            if (neuerHexwert != hexFarbe)
            {
                hexFarbe = neuerHexwert;
                HexFarbeUebernehmen();
            }

            Rect vorschauRect = GUILayoutUtility.GetRect(44f, 20f, GUILayout.Width(44f));
            EditorGUI.DrawRect(vorschauRect, schriftfarbe);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void AnwendenButtonZeichnen()
        {
            bool kannAnwenden = ausgewaehlteObjekte.Count > 0 && gefundeneTexte.Count > 0;

            using (new EditorGUI.DisabledScope(!kannAnwenden))
            {
                if (GUILayout.Button("Farbe anwenden", Styles.Hauptbutton))
                {
                    FarbeAnwenden();
                }
            }
        }

        private void AuswahlAktualisieren()
        {
            ausgewaehlteObjekte.Clear();

            foreach (GameObject objekt in Selection.gameObjects)
            {
                if (objekt != null && IstHierarchyObjekt(objekt))
                {
                    ausgewaehlteObjekte.Add(objekt);
                }
            }

            TexteInAuswahlFinden();
            Repaint();
        }

        private void TexteInAuswahlFinden()
        {
            gefundeneTexte.Clear();
            HashSet<int> bereitsGefunden = new HashSet<int>();

            foreach (GameObject objekt in ausgewaehlteObjekte)
            {
                TextMeshPro[] textMeshProTexte =
                    objekt.GetComponentsInChildren<TextMeshPro>(true);
                TextMeshProUGUI[] textMeshProUiTexte =
                    objekt.GetComponentsInChildren<TextMeshProUGUI>(true);
                Text[] klassischeTexte =
                    objekt.GetComponentsInChildren<Text>(true);

                KomponentenHinzufuegen(textMeshProTexte, bereitsGefunden);
                KomponentenHinzufuegen(textMeshProUiTexte, bereitsGefunden);
                KomponentenHinzufuegen(klassischeTexte, bereitsGefunden);
            }
        }

        private void KomponentenHinzufuegen<T>(
            T[] komponenten,
            HashSet<int> bereitsGefunden) where T : Component
        {
            foreach (T komponente in komponenten)
            {
                if (komponente != null && bereitsGefunden.Add(komponente.GetInstanceID()))
                {
                    gefundeneTexte.Add(komponente);
                }
            }
        }

        private void FarbeAnwenden()
        {
            AuswahlAktualisieren();

            if (ausgewaehlteObjekte.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Schriftfarben",
                    "Kein Hierarchy-Objekt ausgewählt.",
                    "OK");
                return;
            }

            if (gefundeneTexte.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Schriftfarben",
                    "Keine unterstützten Textkomponenten in der aktuellen Auswahl gefunden.",
                    "OK");
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGruppe = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);

            int geaendert = 0;
            int fehler = 0;
            HashSet<int> geaenderteSzenen = new HashSet<int>();

            foreach (Component textKomponente in gefundeneTexte)
            {
                try
                {
                    if (!MussFarbeGeaendertWerden(textKomponente))
                    {
                        continue;
                    }

                    Undo.RecordObject(textKomponente, UndoName);
                    EchteFarbeSetzen(textKomponente, schriftfarbe);
                    EditorUtility.SetDirty(textKomponente);

                    if (PrefabUtility.IsPartOfPrefabInstance(textKomponente))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(textKomponente);
                    }

                    SzeneAlsGeaendertMarkieren(textKomponente, geaenderteSzenen);
                    geaendert++;
                }
                catch (Exception exception)
                {
                    fehler++;
                    Debug.LogError(
                        "Schriftfarben: Die Farbe von '" + textKomponente.name +
                        "' (" + textKomponente.GetType().FullName +
                        ") konnte nicht geändert werden.\n" + exception,
                        textKomponente);
                }
            }

            Undo.CollapseUndoOperations(undoGruppe);
            SceneView.RepaintAll();
            EditorApplication.QueuePlayerLoopUpdate();
            AuswahlAktualisieren();
            RueckmeldungAnzeigen(geaendert, fehler);
        }

        private bool MussFarbeGeaendertWerden(Component textKomponente)
        {
            TextMeshPro textMeshPro = textKomponente as TextMeshPro;
            if (textMeshPro != null)
            {
                return textMeshPro.color != schriftfarbe;
            }

            TextMeshProUGUI textMeshProUi = textKomponente as TextMeshProUGUI;
            if (textMeshProUi != null)
            {
                return textMeshProUi.color != schriftfarbe;
            }

            Text klassischerText = textKomponente as Text;
            if (klassischerText != null)
            {
                return klassischerText.color != schriftfarbe;
            }

            throw new NotSupportedException(
                "Nicht unterstützte Textkomponente: " + textKomponente.GetType().FullName);
        }

        private static void EchteFarbeSetzen(Component textKomponente, Color neueFarbe)
        {
            TextMeshPro textMeshPro = textKomponente as TextMeshPro;
            if (textMeshPro != null)
            {
                textMeshPro.color = neueFarbe;
                return;
            }

            TextMeshProUGUI textMeshProUi = textKomponente as TextMeshProUGUI;
            if (textMeshProUi != null)
            {
                textMeshProUi.color = neueFarbe;
                return;
            }

            Text klassischerText = textKomponente as Text;
            if (klassischerText != null)
            {
                klassischerText.color = neueFarbe;
                return;
            }

            throw new NotSupportedException(
                "Nicht unterstützte Textkomponente: " + textKomponente.GetType().FullName);
        }

        private static void SzeneAlsGeaendertMarkieren(
            Component textKomponente,
            HashSet<int> geaenderteSzenen)
        {
            Scene szene = textKomponente.gameObject.scene;
            if (szene.IsValid() &&
                szene.isLoaded &&
                geaenderteSzenen.Add(szene.handle))
            {
                EditorSceneManager.MarkSceneDirty(szene);
            }
        }

        private void RueckmeldungAnzeigen(int geaendert, int fehler)
        {
            if (geaendert > 0 && fehler == 0)
            {
                EditorUtility.DisplayDialog(
                    "Schriftfarben",
                    ErfolgsText(geaendert),
                    "OK");
                return;
            }

            if (geaendert > 0)
            {
                EditorUtility.DisplayDialog(
                    "Schriftfarben",
                    ErfolgsText(geaendert) + " " +
                    fehler + " Fehler – siehe Konsole für Details.",
                    "OK");
                return;
            }

            if (fehler > 0)
            {
                EditorUtility.DisplayDialog(
                    "Schriftfarben",
                    "Die Farbe konnte nicht angewendet werden. Siehe Konsole für Details.",
                    "OK");
                return;
            }

            EditorUtility.DisplayDialog(
                "Schriftfarben",
                "Alle gefundenen Texte verwenden bereits die ausgewählte Farbe.",
                "OK");
        }

        private static string ErfolgsText(int anzahl)
        {
            return anzahl == 1
                ? "1 Textkomponente erfolgreich geändert."
                : anzahl + " Textkomponenten erfolgreich geändert.";
        }

        private void HexFarbeUebernehmen()
        {
            string wert = hexFarbe.Trim();
            if (!wert.StartsWith("#", StringComparison.Ordinal))
            {
                wert = "#" + wert;
            }

            Color geleseneFarbe;
            if (ColorUtility.TryParseHtmlString(wert, out geleseneFarbe))
            {
                schriftfarbe = geleseneFarbe;
            }
        }

        private string AuswahlStatusText()
        {
            if (ausgewaehlteObjekte.Count == 0)
            {
                return "Kein Objekt ausgewählt";
            }

            return ausgewaehlteObjekte.Count == 1
                ? "1 Objekt ausgewählt"
                : ausgewaehlteObjekte.Count + " Objekte ausgewählt";
        }

        private string TextStatusText()
        {
            if (ausgewaehlteObjekte.Count == 0)
            {
                return "Keine unterstützten Textkomponenten gefunden";
            }

            if (gefundeneTexte.Count == 0)
            {
                return "Keine unterstützten Textkomponenten gefunden";
            }

            return gefundeneTexte.Count == 1
                ? "1 Textkomponente gefunden"
                : gefundeneTexte.Count + " Textkomponenten gefunden";
        }

        private static bool IstHierarchyObjekt(GameObject objekt)
        {
            Scene szene = objekt.scene;
            return szene.IsValid() && szene.isLoaded;
        }

        private static class Styles
        {
            private static GUIStyle karte;
            private static GUIStyle titel;
            private static GUIStyle beschreibung;
            private static GUIStyle abschnittstitel;
            private static GUIStyle status;
            private static GUIStyle hauptbutton;

            public static GUIStyle Karte
            {
                get
                {
                    if (karte == null)
                    {
                        karte = new GUIStyle(EditorStyles.helpBox)
                        {
                            padding = new RectOffset(12, 12, 10, 10)
                        };
                    }

                    return karte;
                }
            }

            public static GUIStyle Titel
            {
                get
                {
                    if (titel == null)
                    {
                        titel = new GUIStyle(EditorStyles.boldLabel)
                        {
                            fontSize = 15
                        };
                    }

                    return titel;
                }
            }

            public static GUIStyle Beschreibung
            {
                get
                {
                    if (beschreibung == null)
                    {
                        beschreibung = new GUIStyle(EditorStyles.wordWrappedLabel)
                        {
                            fontSize = 11,
                            wordWrap = true
                        };
                    }

                    return beschreibung;
                }
            }

            public static GUIStyle Abschnittstitel
            {
                get
                {
                    if (abschnittstitel == null)
                    {
                        abschnittstitel = new GUIStyle(EditorStyles.boldLabel)
                        {
                            fontSize = 12
                        };
                    }

                    return abschnittstitel;
                }
            }

            public static GUIStyle Status
            {
                get
                {
                    if (status == null)
                    {
                        status = new GUIStyle(EditorStyles.label)
                        {
                            wordWrap = true
                        };
                    }

                    return status;
                }
            }

            public static GUIStyle Hauptbutton
            {
                get
                {
                    if (hauptbutton == null)
                    {
                        hauptbutton = new GUIStyle(GUI.skin.button)
                        {
                            fixedHeight = 38f,
                            fontStyle = FontStyle.Bold,
                            fontSize = 12
                        };
                    }

                    return hauptbutton;
                }
            }
        }
    }
}
