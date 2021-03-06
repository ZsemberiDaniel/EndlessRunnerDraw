﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace OneDollar {
    [CreateAssetMenu(menuName = "Templates/TemplateGroup")]
    public class TemplateGroup : ScriptableObject {

        [SerializeField]
        internal List<Template> templates = new List<Template>();
        /// <summary>
        /// The templates in this group in a modifiable form
        /// </summary>
        public List<Template> ModifiableTemplates {
            get { return templates; }
        }
        /// <summary>
        /// Templates in a non-modifiable form
        /// </summary>
        public ReadOnlyCollection<Template> Templates {
            get { return templates.AsReadOnly(); }
        }

        /// <summary>
        /// Learns a template from the given points. Each point is a float pair (0,1) (2,3)...
        /// </summary>
        /// <exception cref="ArgumentNullException">Name cannot be empty or null</exception>
        public void Learn(string name, params float[] points) {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("Name cannot be empty or null!");

            Vector2[] p = new Vector2[points.Length / 2];

            for (int i = 0; i < points.Length; i += 2)
                p[i / 2] = new Vector2(points[i], points[i + 1]);

            Learn(name, p);
        }

        /// <summary>
        /// Learns a template from the given points
        /// </summary>
        /// <exception cref="ArgumentNullException">Name cannot be empty or null</exception>
        public void Learn(string name, params Vector2[] points) {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("Name cannot be empty or null!");
            if (ContainsName(name)) return;

            templates.Add(new Template(
                name, ShapeRecognizer.DoEverything(new List<Vector2>(points))
            ));
        }

        private bool ContainsName(string name) {
            for (int i = 0; i < templates.Count; i++)
                if (templates[i].Name == name)
                    return true;

            return false;
        }


        [Serializable]
        public class Template {
            [SerializeField]
            private string name;
            public string Name { get { return name; } }
            [SerializeField]
            private List<Vector2> points;
            public List<Vector2> Points { get { return points; } }

            /// <exception cref="ArgumentNullException">Name cannot be empty or null. Points cannot be null</exception>
            public Template(string name, List<Vector2> points) {
                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("Name cannot be empty or null!");
                if (points == null) throw new ArgumentNullException("Points cannot be null!");

                this.name = name;
                this.points = points;
            }
        }

    }
}