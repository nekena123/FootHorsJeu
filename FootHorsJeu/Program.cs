using System;
using System.Collections.Generic;
using OpenCvSharp;
using FootHorsJeu.Model;
using FootHorsJeu.Services;

namespace FootHorsJeu
{
    class Program
    {
        static void Main(string[] args)
        {
            // Charger l'image
            string imagePath = @"D:\GitHub\FootHorsJeu\football.jpg";
            Mat image = Cv2.ImRead(imagePath);

            if (image.Empty())
            {
                Console.WriteLine("Erreur lors du chargement de l'image.");
                return;
            }

            // Assigner l'image au contexte global
            Global.Image = image;

            // Plages de couleurs pour détecter les cercles (ajustez selon les couleurs des joueurs)
            var colorRanges = new Dictionary<string, (Scalar lower, Scalar upper)>
            {
                { "Red", (new Scalar(0, 50, 50), new Scalar(10, 255, 255)) },
                { "Blue", (new Scalar(100, 150, 50), new Scalar(140, 255, 255)) },
                { "Noir", (new Scalar(0, 0, 0), new Scalar(180, 255, 50)) }
            };

            // Appeler la méthode DetectCircles pour détecter les cercles (joueurs et ballons)
            var detectedObjects = Circle.DetectCircles(image, colorRanges);

            var joueurs = new List<Joueur>();
            Ballon ballon = null;

            // Traiter les objets détectés
            foreach (var obj in detectedObjects)
            {
                if (obj is Ballon b)
                {
                    Console.WriteLine($"Ballon détecté à la position ({b.X}, {b.Y})");
                    ballon = b;
                }
                else if (obj is Joueur joueur)
                {
                    Console.WriteLine($"Joueur de l'équipe {joueur.Team} détecté à la position ({joueur.X}, {joueur.Y})");
                    joueurs.Add(joueur);
                }
            }

            // Analyser la situation de hors-jeu
            AnalyseHorsJeu.AnalyserSituation(joueurs, ballon, out var possesseurBallon, out var horsJeu, out var bienPlaces);

            // Afficher l'image annotée
            Cv2.ImShow("Résultat", Global.Image);
            Cv2.WaitKey(0);

            // Libérer les ressources
            image.Dispose();
            Cv2.DestroyAllWindows();
        }
    }
}
