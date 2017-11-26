﻿using System;
using ArcObjectConverters.GeoJson;
using ESRI.ArcGIS.Geometry;
using Newtonsoft.Json;
using VL.ArcObjectsApi;
using VL.ArcObjectsApi.Xunit2;

namespace ArcObjectJsonConverters.Tests.GeoJson
{
    public class PolylineClassToMultiLineStringTests
    {
        private readonly IArcObjectFactory _factory = new ClientArcObjectFactory();

        [ArcObjectsTheory, ArcObjectsConventions(32188)]
        public void NonTouchingPathsReturnsJson(PolylineGeoJsonConverter sut, ILine line, ILine otherLine, ISpatialReference spatialReference)
        {
            object missing = Type.Missing;

            var path1 = (ISegmentCollection)_factory.CreateObject<Path>();
            path1.AddSegment((ISegment)line, missing, missing);

            var path2 = (ISegmentCollection)_factory.CreateObject<Path>();
            path2.AddSegment((ISegment)otherLine, missing, missing);

            var polyline = (IGeometryCollection)_factory.CreateObject<Polyline>();
            polyline.AddGeometry((IGeometry)path1);
            polyline.AddGeometry((IGeometry)path2);

            ((IGeometry)polyline).SpatialReference = spatialReference;

            var actual = JsonConvert.SerializeObject(polyline, Formatting.Indented, sut);
            var expected = $@"{{
  ""type"": ""MultiLineString"",
  ""coordinates"": [
    [
      [
        {line.FromPoint.X.ToJsonString()},
        {line.FromPoint.Y.ToJsonString()}
      ],
      [
        {line.ToPoint.X.ToJsonString()},
        {line.ToPoint.Y.ToJsonString()}
      ]
    ],
    [
      [
        {otherLine.FromPoint.X.ToJsonString()},
        {otherLine.FromPoint.Y.ToJsonString()}
      ],
      [
        {otherLine.ToPoint.X.ToJsonString()},
        {otherLine.ToPoint.Y.ToJsonString()}
      ]
    ]
  ]
}}";

            JsonAssert.Equal(expected, actual);
        }
    }
}
