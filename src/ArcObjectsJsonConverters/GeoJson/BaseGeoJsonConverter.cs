﻿using System;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;

namespace ArcObjectConverters.GeoJson
{
    public abstract class BaseGeoJsonConverter : JsonConverter
    {
        private readonly GeoJsonSerializerSettings _serializerSettings;

        protected BaseGeoJsonConverter()
            : this(new GeoJsonSerializerSettings())
        {
        }

        protected BaseGeoJsonConverter(GeoJsonSerializerSettings serializerSettings)
        {
            if (serializerSettings == null) throw new ArgumentNullException(nameof(serializerSettings));
            _serializerSettings = serializerSettings;
        }

        /// <summary>
        /// Prepare the geometry (or a copy of itself) to be serialized. Depending on <see cref="GeoJsonSerializerSettings"/>,
        /// the geometry might be altered, cloned and generalized by this function.
        ///  
        /// <see cref="IGeometry"/> operations like <see cref="IGeometry.Project(ISpatialReference)"/> can
        /// have side effets (altering the input object). If <c>true</c>, geometries will not be cloned,
        /// increasing performance, if <c>false</c>, no side effects will happen, at a cost of lower
        /// performance.
        /// </summary>
        protected virtual object PrepareGeometry(object value)
        {
            if (_serializerSettings.SerializerHasSideEffects || value == null)
            {
                return value;
            }

            return ((IClone) value).Clone();
        }

        protected void WritePositionArray(JsonWriter writer, IPoint value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(Math.Round(value.X, _serializerSettings.Precision));
            writer.WriteValue(Math.Round(value.Y, _serializerSettings.Precision));

            if (_serializerSettings.Dimensions == DimensionHandling.XYZ)
            {
                var zAware = (IZAware) value;
                var z = !zAware.ZAware || double.IsNaN(value.Z)
                    ? _serializerSettings.DefaultZValue
                    : Math.Round(value.Z, _serializerSettings.Precision);

                writer.WriteValue(z);
            }

            writer.WriteEndArray();
        }

        protected void WritePointObject(JsonWriter writer, IPoint point, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue("Point");

            writer.WritePropertyName("coordinates");
            WritePositionArray(writer, point, serializer);

            writer.WriteEndObject();
        }

        protected void WriteLineStringCoordinatesArray(JsonWriter writer, IPointCollection lineString, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            for (int i = 0, n = lineString.PointCount; i < n; i++)
            {
                WritePositionArray(writer, lineString.Point[i], serializer);
            }

            writer.WriteEndArray();
        }

        protected void WriteLineStringObject(JsonWriter writer, IPointCollection lineString, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue("LineString");

            writer.WritePropertyName("coordinates");
            WriteLineStringCoordinatesArray(writer, lineString, serializer);

            writer.WriteEndObject();
        }
    }
}