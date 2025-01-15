using System;
using System.Collections.Generic;
using OpenCvSharp;
using FootHorsJeu.Model;

namespace FootHorsJeu.Model
{
    public class Circle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Radius { get; set; }
        public string Color { get; set; }

        // Méthode pour détecter les cercles (joueur ou ballon)
        public static List<object> DetectCircles(Mat image, Dictionary<string, (Scalar lower, Scalar upper)> colorRanges)
        {
            var detectedObjects = new List<object>(); // Liste pour stocker à la fois les joueurs et les ballons

            // Convertir l'image en HSV pour détecter les couleurs
            Mat hsvImage = new Mat();
            Cv2.CvtColor(image, hsvImage, ColorConversionCodes.BGR2HSV);

            foreach (var colorRange in colorRanges)
            {
                string color = colorRange.Key;
                Scalar lower = colorRange.Value.lower;
                Scalar upper = colorRange.Value.upper;

                // Créer un masque pour la plage de couleur
                Mat mask = new Mat();
                Cv2.InRange(hsvImage, lower, upper, mask);

                // Appliquer un flou pour réduire le bruit
                Cv2.MedianBlur(mask, mask, 5);

                // Détecter les contours des cercles
                Point[][] contours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(mask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                foreach (var contour in contours)
                {
                    // Approximer chaque contour à un cercle
                    var moments = Cv2.Moments(contour);
                    if (moments.M00 > 0)
                    {
                        int cx = (int)(moments.M10 / moments.M00);
                        int cy = (int)(moments.M01 / moments.M00);

                        // Calculer un rayon approximatif (en utilisant la bounding box)
                        var boundingRect = Cv2.BoundingRect(contour);
                        int radius = boundingRect.Width / 2;

                        // Créer l'objet cercle
                        var circle = new Circle
                        {
                            X = cx,
                            Y = cy,
                            Radius = radius,
                            Color = color
                        };

                        // Ajouter à la liste des objets détectés en fonction de la couleur
                        if (color == "Noir")
                        {
                            // Si c'est un cercle noir, c'est un ballon
                            detectedObjects.Add(new Ballon { X = cx, Y = cy });
                        }
                        else
                        {
                            // Sinon, c'est un joueur avec la couleur comme équipe
                            detectedObjects.Add(new Joueur { X = cx, Y = cy, Team = color });
                        }

                        // Dessiner le cercle et afficher l'initiale de la couleur
                        // string label = $"{color[0]}";
                        Cv2.Circle(image, new Point(cx, cy), radius, Scalar.Black, 2);
                        // Cv2.PutText(image, label, new Point(cx - 10, cy - 10),
                        //     HersheyFonts.HersheySimplex, 0.5, Scalar.Green, 2);
                    }
                }

                mask.Dispose();
            }

            hsvImage.Dispose();
            return detectedObjects; // Retourne la liste des objets détectés (joueurs ou ballons)
        }
    }
}
