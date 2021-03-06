﻿using System;
using System.Collections.Generic;
using System.Linq;
using Utf8Json;
using ViaJsonBuilder.Extensions;
using ViaJsonBuilder.Models.Via;

namespace ViaJsonBuilder.Models.Json
{
    public class ViaBuilder : IJsonBuilder
    {
        public string Build(JsonBuildingContext context)
        {
            var model = this.GetViaModel(context);

            return JsonSerializer.ToJsonString(model);
        }

        private ViaModel GetViaModel(JsonBuildingContext context)
        {
            var name = context.Name;
            var vendorId = context.VendorId;
            var productId = context.ProductId;

            return new ViaModel
            {
                Name = name,
                VenderId = vendorId,
                ProductId = productId,
                Lighting = this.GetLighting(context),
                Matrix = this.GetMatrix(context),
                Layouts = this.GetLayouts(context),
            };
        }

        private Lighting GetLighting(JsonBuildingContext context)
        {
            if (Enum.TryParse<Lighting>(context.Lighting, out var lighting))
            {
                return lighting;
            }

            return Lighting.none;
        }

        private Layouts GetLayouts(JsonBuildingContext context)
        {
            return new Layouts
            {
                Labels = this.GetLabels(context),
                Keymap = this.GetKeymap(context),
            };
        }

        private IEnumerable<dynamic> GetLabels(JsonBuildingContext context)
        {
            var labels = context.Labels;

            if (labels.IsNullOrWhiteSpace())
            {
                return default;
            }

            var formatted = labels.Split(",")
                .Select(x =>
                {
                    if (x.StartsWith("[") && !x.EndsWith("]"))
                    {
                        return "[" + x.TrimStart('[').Enclose(@"""");
                    }

                    if (!x.StartsWith("[") && x.EndsWith("]"))
                    {
                        return x.TrimEnd(']').Enclose(@"""") + "]";
                    }

                    return x.Enclose(@"""");
                })
                .JoinComma();

            if(!formatted.StartsWith("["))
            {
                formatted = $"[{formatted}]";
            }

            return JsonSerializer.Deserialize<IEnumerable<dynamic>>(formatted);
        }

        private dynamic GetKeymap(JsonBuildingContext context)
        {
            var keymap = context.Keymap;

            if (keymap.HasMeaningfulValue())
            {
                return JsonSerializer.Deserialize<dynamic>(keymap);
            }

            return default;
        }

        private Matrix GetMatrix(JsonBuildingContext context)
        {
            var matrix = new Matrix();

            if (int.TryParse(context.Rows, out var rows))
            {
                matrix.Rows = rows;
            }

            if (int.TryParse(context.Cols, out var cols))
            {
                matrix.Cols = cols;
            }

            return matrix;
        }
    }
}
