﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamControl.Core.Services
{
    public class PlaceholderService : IPlaceholderService
    {
        public Dictionary<string, string> Placeholders { get; set; }

        public PlaceholderService()
        {
            Placeholders = new Dictionary<string, string>();
        }

        public string ReplacePlaceholders(string text, params object[] instances)
        {
            return ReplacePlaceholders(new string[] { text }, instances).First();
        }

        public IEnumerable<string> ReplacePlaceholders(IEnumerable<string> texts, params object[] instances)
        {
            Dictionary<string, string> propertyPlaceholders = GetPropertyPlaceholders(instances);

            foreach (var item in texts)
            {
                StringBuilder sb = new StringBuilder(item);
                foreach (var placeholder in new[] { ReplaceDatePlaceholder(item) }
                                                .Concat(Placeholders)
                                                .Concat(propertyPlaceholders))
                    sb.Replace("<"+placeholder.Key+">", placeholder.Value);
                yield return sb.ToString();
            }
        }

        private KeyValuePair<string, string> ReplaceDatePlaceholder(string item)
        {
            int start = item.IndexOf("<Date{") + 6;
            if (start < 6)
                return new KeyValuePair<string, string>("---", "---");

            int end = item.IndexOf("}", start);
            var format = item.Substring(start, end - start);
            return new KeyValuePair<string, string>("Date{" + format + "}", DateTime.Now.ToString(format));
        }

        private static Dictionary<string, string> GetPropertyPlaceholders(object[] instances)
        {
            Dictionary<string, string> propertyPlaceholders = new Dictionary<string, string>();
            foreach (var item in instances.Reverse())
                foreach (var property in item.GetType().GetProperties())
                    propertyPlaceholders[property.Name] = property.GetValue(item).ToString();
            return propertyPlaceholders;
        }

        public void AddAll(IEnumerable<KeyValuePair<string, string>> placeholders)
        {
            foreach (var item in placeholders)
            {
                Placeholders[item.Key] = item.Value;
            }
        }
    }
}
