﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SpeakEasy
{
    public class Resource 
    {
        private readonly string[] segmentNames;

        private readonly List<Parameter> parameters = new List<Parameter>();

        public Resource(string path)
        {
            Path = path.TrimEnd('/');

            segmentNames = ExtractSegments();
        }

        public string Path { get; private set; }

        public int NumSegments
        {
            get { return segmentNames.Length; }
        }

        public bool HasSegments
        {
            get { return segmentNames.Any(); }
        }

        public IEnumerable<Parameter> Parameters
        {
            get { return parameters; }
        }

        public bool HasParameters
        {
            get { return parameters.Any(); }
        }

        public int NumParameters
        {
            get { return parameters.Count; }
        }

        public bool HasSegment(string name)
        {
            return segmentNames.Contains(name);
        }

        private string[] ExtractSegments()
        {
            var segmentMatches = Regex.Matches(Path, ":([a-zA-Z-]+)");

            return segmentMatches
                .Cast<Match>()
                .Select(m => m.Groups[1].Value.ToLower())
                .ToArray();
        }

        public void AddParameter(string name, object value)
        {
            parameters.Add(new Parameter(name, value));
        }

        public bool HasParameter(string name)
        {
            return parameters.Any(p => p.Name == name);
        }

        public Resource Merge(object segments, bool shouldMergeProperties = true)
        {
            if (!HasSegments && segments == null)
            {
                return this;
            }

            if (segments == null)
            {
                var message = string.Format(
                    "The resource {0} requires the following segments, but none were given: {1}",
                    Path,
                    string.Join(", ", segmentNames));

                throw new ArgumentException(message);
            }

            var properties = segments
                .GetType()
                .GetProperties()
                .ToDictionary(p => p.Name.ToLower());

            var mergedResource = MergeUrlSegments(segments, properties);

            if (!shouldMergeProperties)
            {
                return mergedResource;
            }

            return AddMergedParameters(mergedResource, segments, properties);
        }

        private Resource AddMergedParameters(Resource mergedResource, object segments, Dictionary<string, PropertyInfo> properties)
        {
            foreach (var property in properties.Values)
            {
                var propertyValue = property.GetValue(segments, new object[0]);
                mergedResource.AddParameter(property.Name, propertyValue);
            }

            return mergedResource;
        }

        private Resource MergeUrlSegments(object segments, IDictionary<string, PropertyInfo> properties)
        {
            var merged = Path;

            foreach (var segmentName in segmentNames)
            {
                PropertyInfo property;
                if (!properties.TryGetValue(segmentName, out property))
                {
                    throw new ArgumentException("Could not find a property matching segment: " + segmentName);
                }

                var propertyValue = property.GetValue(segments, new object[0]);

                merged = merged.Replace(":" + segmentName, propertyValue.ToString());

                properties.Remove(segmentName);
            }

            return new Resource(merged);
        }

        public Resource Append(Resource resource)
        {
            var combined =
                resource.Path.StartsWith("/")
                    ? string.Concat(Path, resource.Path)
                    : string.Concat(Path, "/", resource.Path);

            return new Resource(combined);
        }

        public Resource Append(string resource)
        {
            return Append(new Resource(resource));
        }

        public string GetEncodedParameters()
        {
            var encodableParameters = Parameters.Where(p => p.HasValue).Select(p => p.ToQueryString());
            return string.Join("&", encodableParameters);
        }

        public override string ToString()
        {
            var formattedParameters = parameters.Where(p => p.HasValue).Select(s => s.ToQueryString()).ToList();
            var parameterList = formattedParameters.Any() ? string.Join(", ", formattedParameters) : "none";

            return string.Format("[Resource Path={0}, Parameters={1}]", Path, parameterList);
        }
    }
}
