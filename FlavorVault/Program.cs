using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace KochrezeptVerwalter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            RezeptManager manager = new RezeptManager();

            manager.Start();
        }
    }

    class RezeptManager
    {
        private List<Rezept> _rezepte;
        private string _dateiPfad;
        private Random _zufallsgenerator;
        private RezeptSerializer _serializer;
        private BenutzerSchnittstelle _ui;

        public RezeptManager()
        {
            _rezepte = new List<Rezept>();
            _dateiPfad = "rezepte.json";
            _zufallsgenerator = new Random();
            _serializer = new RezeptSerializer(_dateiPfad);
            _ui = new BenutzerSchnittstelle();
        }

        public void Start()
        {
            LadeRezepte();

            bool beenden = false;
            while (!beenden)
            {
                _ui.ZeigeMenü();
                string auswahl = Console.ReadLine();

                switch (auswahl)
                {
                    case "1":
                        RezeptHinzufügen();
                        break;
                    case "2":
                        AlleRezepteAnzeigen();
                        break;
                    case "3":
                        ZufälligesRezeptAnzeigen();
                        break;
                    case "4":
                        ZufälligesRezeptMitFilter();
                        break;
                    case "5":
                        RestverwendungFinden();
                        break;
                    case "6":
                        beenden = true;
                        break;
                    default:
                        _ui.ZeigeFehler("Ungültige Auswahl. Bitte versuchen Sie es erneut.");
                        break;
                }

                if (!beenden)
                {
                    _ui.WarteTaste();
                    Console.Clear();
                }
            }
        }

        private void LadeRezepte()
        {
           
                _rezepte = _serializer.LadeRezepte();
                _ui.ZeigeNachricht($"{_rezepte.Count} Rezepte erfolgreich geladen.");
            
        }

        private void SpeichereRezepte()
        {
            
            
                _serializer.SpeichereRezepte(_rezepte);
                _ui.ZeigeNachricht("Rezepte erfolgreich gespeichert.");
            
             
  
        }

        private void RezeptHinzufügen()
        {
            Console.Clear();
            _ui.ZeigeÜberschrift("Neues Rezept hinzufügen");

            _ui.ZeigeZurückOption();
            if (_ui.PrüfeZurückAuswahl())
            {
                return;
            }

            string name = _ui.LiesTextEingabe("Name des Rezepts: ");
            string beschreibung = _ui.LiesTextEingabe("Kurze Beschreibung: ");

            int zubereitungszeit = _ui.LiesZahlEingabe("Zubereitungszeit (in Minuten): ", 1, int.MaxValue);
            int schwierigkeit = _ui.LiesZahlEingabe("Schwierigkeitsgrad (1-5): ", 1, 5);

            List<string> zutaten = _ui.LiesListe("Geben Sie die Zutaten ein (leere Zeile zum Beenden):");
            List<string> schritte = _ui.LiesListe("Geben Sie die Zubereitungsschritte ein (leere Zeile zum Beenden):");

            var neuesRezept = new Rezept(name, beschreibung, zubereitungszeit, schwierigkeit, zutaten, schritte);

            _rezepte.Add(neuesRezept);
            SpeichereRezepte();
            _ui.ZeigeNachricht($"Rezept '{name}' wurde erfolgreich hinzugefügt!");
        }

        private void AlleRezepteAnzeigen()
        {
            Console.Clear();
            _ui.ZeigeÜberschrift("Alle Rezepte");

            if (_rezepte.Count == 0)
            {
                _ui.ZeigeNachricht("Keine Rezepte vorhanden.");
                return;
            }

            for (int i = 0; i < _rezepte.Count; i++)
            {
                _ui.ZeigeRezeptKurzinfo(i + 1, _rezepte[i]);
            }

            int auswahl = _ui.LiesZahlEingabeOptional("\nGeben Sie die Nummer eines Rezepts ein, um Details anzuzeigen (oder 0 zum Zurückkehren): ", 0, _rezepte.Count);
            if (auswahl > 0)
            {
                ZeigeRezeptDetailsMitZurückOption(_rezepte[auswahl - 1]);
            }
        }

        private void ZeigeRezeptDetailsMitZurückOption(Rezept rezept)
        {
            bool zurückZurListe = false;

            while (!zurückZurListe)
            {
                Console.Clear();
                _ui.ZeigeRezeptDetails(rezept);

                Console.WriteLine("\nNavigationsoptionen:");
                Console.WriteLine("1. Zurück zur Rezeptliste");
                Console.WriteLine("2. Zurück zum Hauptmenü");
                Console.Write("Ihre Auswahl: ");

                string auswahl = Console.ReadLine();

                switch (auswahl)
                {
                    case "1":
                        zurückZurListe = true;
                        Console.Clear();
                        AlleRezepteAnzeigen();
                        return;
                    case "2":
                        zurückZurListe = true;
                        return;
                    default:
                        _ui.ZeigeFehler("Ungültige Auswahl. Bitte versuchen Sie es erneut.");
                        _ui.WarteTaste();
                        break;
                }
            }
        }

        private void ZufälligesRezeptAnzeigen()
        {
            if (_rezepte.Count == 0)
            {
                _ui.ZeigeNachricht("Keine Rezepte vorhanden, um einen Vorschlag zu machen.");
                return;
            }

            int zufallsIndex = _zufallsgenerator.Next(_rezepte.Count);
            _ui.ZeigeNachricht("Hier ist ein zufälliger Rezeptvorschlag für Sie:");
            ZeigeRezeptDetailsMitZurückOption(_rezepte[zufallsIndex]);
        }

        private void ZufälligesRezeptMitFilter()
        {
            if (_rezepte.Count == 0)
            {
                _ui.ZeigeNachricht("Keine Rezepte vorhanden, um einen Vorschlag zu machen.");
                return;
            }

            Console.Clear();
            _ui.ZeigeÜberschrift("Zufälliges Rezept mit Filter");

            _ui.ZeigeZurückOption();
            if (_ui.PrüfeZurückAuswahl())
            {
                return;
            }

            _ui.ZeigeNachricht("Wählen Sie Ihre Filter:");

            int maxZeit = _ui.LiesZahlEingabeOptional("Maximale Zubereitungszeit in Minuten (0 für keine Begrenzung): ", 0, int.MaxValue);
            int maxSchwierigkeit = _ui.LiesZahlEingabeOptional("Maximaler Schwierigkeitsgrad (1-5, 0 für keine Begrenzung): ", 0, 5);
            string zutat = _ui.LiesTextEingabe("Enthaltene Zutat (optional, leer lassen für keine Einschränkung): ");

            RezeptFilter filter = new RezeptFilter();
            var gefilterteRezepte = filter.FiltereRezepte(_rezepte, maxZeit, maxSchwierigkeit, zutat);

            if (gefilterteRezepte.Count == 0)
            {
                _ui.ZeigeNachricht("Keine Rezepte entsprechen Ihren Filterkriterien.");
                return;
            }

            int zufallsIndex = _zufallsgenerator.Next(gefilterteRezepte.Count);
            _ui.ZeigeNachricht("\nHier ist ein zufälliger Rezeptvorschlag basierend auf Ihren Filtern:");
            ZeigeRezeptDetailsMitZurückOption(gefilterteRezepte[zufallsIndex]);
        }

        private void RestverwendungFinden()
        {
            if (_rezepte.Count == 0)
            {
                _ui.ZeigeNachricht("Keine Rezepte vorhanden, um Reste zu verwenden.");
                return;
            }

            Console.Clear();
            _ui.ZeigeÜberschrift("Reste verwenden");

            _ui.ZeigeZurückOption();
            if (_ui.PrüfeZurückAuswahl())
            {
                return;
            }

            string eingabe = _ui.LiesTextEingabe("Geben Sie die Zutaten ein, die Sie verwenden möchten (getrennt durch Komma):");

            var verfügbareZutaten = eingabe.Split(',')
                .Select(z => z.Trim().ToLower())
                .Where(z => !string.IsNullOrWhiteSpace(z))
                .ToList();

            if (verfügbareZutaten.Count == 0)
            {
                _ui.ZeigeNachricht("Keine Zutaten angegeben.");
                return;
            }

            _ui.ZeigeNachricht($"\nSuche nach Rezepten, die folgende Zutaten verwenden: {string.Join(", ", verfügbareZutaten)}");

            RezeptFilter filter = new RezeptFilter();
            var passendeRezepte = filter.FindePassendeRezepteFürZutaten(_rezepte, verfügbareZutaten);

            if (passendeRezepte.Count == 0)
            {
                _ui.ZeigeNachricht("Keine passenden Rezepte gefunden.");
                return;
            }

            _ui.ZeigeNachricht("\nGefundene Rezepte (sortiert nach Übereinstimmung):");
            for (int i = 0; i < passendeRezepte.Count; i++)
            {
                var (rezept, anzahl) = passendeRezepte[i];
                _ui.ZeigeNachricht($"{i + 1}. {rezept.Name} - Verwendet {anzahl} Ihrer Zutaten");
            }

            int auswahl = _ui.LiesZahlEingabeOptional("\nGeben Sie die Nummer eines Rezepts ein, um Details anzuzeigen (oder 0 zum Zurückkehren): ", 0, passendeRezepte.Count);
            if (auswahl > 0)
            {
                ZeigeRezeptDetailsMitZurückOption(passendeRezepte[auswahl - 1].Rezept);
            }
        }
    }

    class RezeptFilter
    {
        public List<Rezept> FiltereRezepte(List<Rezept> rezepte, int maxZeit, int maxSchwierigkeit, string zutat)
        {
            return rezepte.Where(r =>
                (maxZeit <= 0 || r.Zubereitungszeit <= maxZeit) &&
                (maxSchwierigkeit <= 0 || r.Schwierigkeitsgrad <= maxSchwierigkeit) &&
                (string.IsNullOrWhiteSpace(zutat) || r.EnthältZutat(zutat.ToLower()))
            ).ToList();
        }

        public List<(Rezept Rezept, int AnzahlPassenderZutaten)> FindePassendeRezepteFürZutaten(List<Rezept> rezepte, List<string> verfügbareZutaten)
        {
            var ergebnisse = new List<(Rezept Rezept, int AnzahlPassenderZutaten)>();

            foreach (var rezept in rezepte)
            {
                int übereinstimmungen = rezept.ZähleZutatenÜbereinstimmungen(verfügbareZutaten);
                if (übereinstimmungen > 0)
                {
                    ergebnisse.Add((rezept, übereinstimmungen));
                }
            }

            return ergebnisse.OrderByDescending(p => p.AnzahlPassenderZutaten).ToList();
        }
    }

    class BenutzerSchnittstelle
    {
        public void ZeigeMenü()
        {
            ZeigeÜberschrift("Kochrezept-Verwalter");
            Console.WriteLine("1. Neues Rezept hinzufügen");
            Console.WriteLine("2. Alle Rezepte anzeigen");
            Console.WriteLine("3. Zufälliges Rezept vorschlagen");
            Console.WriteLine("4. Zufälliges Rezept mit Filter");
            Console.WriteLine("5. Rezepte für Resteverwertung finden");
            Console.WriteLine("6. Beenden");
            Console.Write("Ihre Auswahl: ");
        }

        public void ZeigeNachricht(string nachricht)
        {
            Console.WriteLine(nachricht);
        }

        public void ZeigeFehler(string fehler)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(fehler);
            Console.ResetColor();
        }

        public void ZeigeÜberschrift(string überschrift)
        {
            Console.WriteLine($"=== {überschrift} ===");
        }

        public void WarteTaste()
        {
            Console.WriteLine("\nDrücken Sie eine beliebige Taste, um fortzufahren...");
            Console.ReadKey();
        }

        public void ZeigeZurückOption()
        {
            Console.WriteLine("Um zum Hauptmenü zurückzukehren, geben Sie '0' ein");
            Console.WriteLine("----------------------------------------------");
        }

        public bool PrüfeZurückAuswahl()
        {
            Console.Write("Möchten Sie fortfahren oder zum Hauptmenü zurückkehren? (Drücken Sie '0' für Hauptmenü oder Enter zum Fortfahren): ");
            string eingabe = Console.ReadLine();
            return eingabe == "0";
        }

        public string LiesTextEingabe(string aufforderung)
        {
            Console.Write(aufforderung);
            string eingabe = Console.ReadLine();

            if (eingabe == "0")
            {
                return "0";
            }

            return eingabe;
        }

        public int LiesZahlEingabe(string aufforderung, int min, int max)
        {
            int ergebnis;
            while (true)
            {
                Console.Write(aufforderung);
                string eingabe = Console.ReadLine();

                if (eingabe == "0")
                {
                    return 0;
                }

                if (int.TryParse(eingabe, out ergebnis) && ergebnis >= min && ergebnis <= max)
                {
                    return ergebnis;
                }
                ZeigeFehler($"Bitte geben Sie eine gültige Zahl zwischen {min} und {max} ein.");
            }
        }

        public int LiesZahlEingabeOptional(string aufforderung, int min, int max)
        {
            int ergebnis;
            while (true)
            {
                Console.Write(aufforderung);
                string eingabe = Console.ReadLine();

                if (int.TryParse(eingabe, out ergebnis) && ergebnis >= min && ergebnis <= max)
                {
                    return ergebnis;
                }
                ZeigeFehler($"Bitte geben Sie eine gültige Zahl zwischen {min} und {max} ein.");
            }
        }

        public List<string> LiesListe(string aufforderung)
        {
            Console.WriteLine(aufforderung);
            Console.WriteLine("(Geben Sie '0' ein, um zum Hauptmenü zurückzukehren)");
            var liste = new List<string>();
            while (true)
            {
                string eingabe = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(eingabe))
                {
                    break;
                }

                if (eingabe == "0")
                {
                    return new List<string>();
                }

                liste.Add(eingabe);
            }
            return liste;
        }

        public void ZeigeRezeptKurzinfo(int nummer, Rezept rezept)
        {
            Console.WriteLine($"{nummer}. {rezept.Name} (Zubereitungszeit: {rezept.Zubereitungszeit} Min, Schwierigkeit: {rezept.Schwierigkeitsgrad}/5)");
        }

        public void ZeigeRezeptDetails(Rezept rezept)
        {
            ZeigeÜberschrift(rezept.Name);
            Console.WriteLine($"Beschreibung: {rezept.Beschreibung}");
            Console.WriteLine($"Zubereitungszeit: {rezept.Zubereitungszeit} Minuten");
            Console.WriteLine($"Schwierigkeitsgrad: {rezept.Schwierigkeitsgrad}/5");

            Console.WriteLine("\nZutaten:");
            foreach (var zutat in rezept.Zutaten)
            {
                Console.WriteLine($"- {zutat}");
            }

            Console.WriteLine("\nZubereitung:");
            for (int i = 0; i < rezept.Zubereitungsschritte.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {rezept.Zubereitungsschritte[i]}");
            }
        }
    }

    class RezeptSerializer
    {
        private string _dateiPfad;

        public RezeptSerializer(string dateiPfad)
        {
            _dateiPfad = dateiPfad;
        }

        public List<Rezept> LadeRezepte()
        {
            if (File.Exists(_dateiPfad))
            {
                string jsonString = File.ReadAllText(_dateiPfad);
                return JsonSerializer.Deserialize<List<Rezept>>(jsonString) ?? new List<Rezept>();
            }
            return new List<Rezept>();
        }

        public void SpeichereRezepte(List<Rezept> rezepte)
        {
            string jsonString = JsonSerializer.Serialize(rezepte, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_dateiPfad, jsonString);
        }
    }

    class Rezept
    {
        public string Name { get; set; }
        public string Beschreibung { get; set; }
        public int Zubereitungszeit { get; set; }
        public int Schwierigkeitsgrad { get; set; }
        public List<string> Zutaten { get; set; }
        public List<string> Zubereitungsschritte { get; set; }

        public Rezept()
        {
            Name = string.Empty;
            Beschreibung = string.Empty;
            Zutaten = new List<string>();
            Zubereitungsschritte = new List<string>();
        }

        public Rezept(string name, string beschreibung, int zubereitungszeit, int schwierigkeitsgrad,
                     List<string> zutaten, List<string> zubereitungsschritte)
        {
            Name = name;
            Beschreibung = beschreibung;
            Zubereitungszeit = zubereitungszeit;
            Schwierigkeitsgrad = schwierigkeitsgrad;
            Zutaten = zutaten;
            Zubereitungsschritte = zubereitungsschritte;
        }

        public bool EnthältZutat(string zutat)
        {
            return Zutaten.Any(z => z.ToLower().Contains(zutat.ToLower()));
        }

        public int ZähleZutatenÜbereinstimmungen(List<string> verfügbareZutaten)
        {
            int übereinstimmungen = 0;
            foreach (var zutat in verfügbareZutaten)
            {
                if (EnthältZutat(zutat))
                {
                    übereinstimmungen++;
                }
            }
            return übereinstimmungen;
        }
    }
}