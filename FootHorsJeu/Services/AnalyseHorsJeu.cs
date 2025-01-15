using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp; // Assurez-vous d'avoir ajouté la référence à OpenCV
using FootHorsJeu.Model;

namespace FootHorsJeu.Services
{
    public class AnalyseHorsJeu
    {
        public static void AnalyserSituation(List<Joueur> joueurs, Ballon ballon, out Joueur possesseurBallon, out List<Joueur> horsJeu, out List<Joueur> bienPlaces)
        {
            possesseurBallon = null;
            horsJeu = new List<Joueur>();
            bienPlaces = new List<Joueur>();

            // Liste des annotations pour dessin après analyse
            var annotations = new List<(int X, int Y, string Text, Scalar Color)>();

            // Trouver le joueur possédant le ballon (le plus proche du ballon)
            possesseurBallon = joueurs.OrderBy(j => Distance(j.X, j.Y, ballon.X, ballon.Y)).First();

            // Identifier l'équipe possédant le ballon
            string equipePossesseur = possesseurBallon.Team;

            // Trouver le joueur ayant la position Y maximale (le plus bas)
            var joueurMaxY = joueurs.OrderByDescending(j => j.Y).First();
            string equipeJoueurMaxY = joueurMaxY.Team; // Récupérer l'équipe du joueur avec Y maximal

            // Trouver le joueur ayant la position Y minimale (le plus haut)
            var joueurMinY = joueurs.OrderBy(j => j.Y).First();
            string equipeJoueurMinY = joueurMinY.Team; // Récupérer l'équipe du joueur avec Y minimal

            // Séparer les joueurs par équipe
            var joueursEquipePossesseur = joueurs.Where(j => j.Team == equipePossesseur).ToList();
            var joueursEquipeAdverse = joueurs.Where(j => j.Team != equipePossesseur).ToList();

            Joueur dernierDefenseur= null;

            if (equipePossesseur == equipeJoueurMinY)
            {
                // Trouver le dernier défenseur de l'équipe adverse (basé sur Y maintenant)
                dernierDefenseur = joueursEquipeAdverse.OrderByDescending(j => j.Y).Skip(1).First();
            }
            else if (equipePossesseur == equipeJoueurMaxY)
            {
                // Trouver le dernier défenseur de l'équipe adverse (basé sur Y maintenant)
                dernierDefenseur = joueursEquipeAdverse.OrderBy(j => j.Y).Skip(1).First();
            }

            // Parcourir les joueurs de l'équipe possédant le ballon
            foreach (var joueur in joueursEquipePossesseur)
            {
                if (joueur == possesseurBallon) continue; // Ignorer le possesseur du ballon

                if (equipePossesseur == equipeJoueurMinY)
                {
                    if (joueur.Y > dernierDefenseur.Y)
                    {
                        // Joueur est hors-jeu
                        horsJeu.Add(joueur);
                        annotations.Add((joueur.X, joueur.Y, "Hj", Scalar.Red));
                    }
                    else if (joueur.Y > possesseurBallon.Y && joueur.Y <= dernierDefenseur.Y)
                    {
                        // Joueur est bien placé
                        bienPlaces.Add(joueur);
                        annotations.Add((joueur.X, joueur.Y, "M", Scalar.Green));
                    }
                }
                else if (equipePossesseur == equipeJoueurMaxY)
                {
                    if (joueur.Y < dernierDefenseur.Y)
                    {
                        // Joueur est hors-jeu
                        horsJeu.Add(joueur);
                        annotations.Add((joueur.X, joueur.Y, "Hj", Scalar.Red));
                    }
                    else if (joueur.Y < possesseurBallon.Y && joueur.Y >= dernierDefenseur.Y)
                    {
                        // Joueur est bien placé
                        bienPlaces.Add(joueur);
                        annotations.Add((joueur.X, joueur.Y, "M", Scalar.Green));
                    }
                }
                
            }

            // Dessiner les annotations
            DessinerAnnotations(annotations);
        }

        // Méthode pour dessiner les annotations
        private static void DessinerAnnotations(List<(int X, int Y, string Text, Scalar Color)> annotations)
        {
            foreach (var (x, y, text, color) in annotations)
            {
                var position = new Point(x, y - 10); // Position légèrement au-dessus du joueur
                Cv2.PutText(Global.Image, text, position, HersheyFonts.HersheySimplex, 0.7, color, 2);
            }
        }

        // Calcul de la distance entre deux points
        private static double Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }
    }

    public static class Global
    {
        public static Mat Image { get; set; }
    }
}
