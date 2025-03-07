using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// Renderes history chart in PNG format.
    /// </summary>
    internal class PngHistoryChartRenderer
    {
        /// <summary>
        /// Renderes the given historic coverages as PNG image.
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        /// <returns>The image in PNG format.</returns>
        public static byte[] RenderHistoryChart(IReadOnlyList<HistoricCoverage> historicCoverages)
        {
            using (Image<Rgba32> image = new Image<Rgba32>(1450, 150))
            using (MemoryStream output = new MemoryStream())
            {
                var grayPen = Pens.Dash(Color.LightGray, 1);
                var redPen = Pens.Solid(Color.ParseHex("cc0000"), 2);
                var bluePen = Pens.Solid(Color.ParseHex("1c2298"), 2);

                var redBrush = Brushes.Solid(Color.ParseHex("cc0000"));
                var blueBrush = Brushes.Solid(Color.ParseHex("1c2298"));

                int numberOfLines = historicCoverages.Count;

                if (numberOfLines == 1)
                {
                    numberOfLines = 2;
                }

                float totalWidth = 1445 - 50;
                float width = totalWidth / (numberOfLines - 1);

                float totalHeight = 115 - 15;

                image.Mutate(ctx =>
                {
                    ctx.Fill(Color.White);

                    ctx.DrawLines(grayPen, new PointF(50, 115), new PointF(1445, 115));
                    ctx.DrawLines(grayPen, new PointF(50, 90), new PointF(1445, 90));
                    ctx.DrawLines(grayPen, new PointF(50, 65), new PointF(1445, 65));
                    ctx.DrawLines(grayPen, new PointF(50, 40), new PointF(1445, 40));
                    ctx.DrawLines(grayPen, new PointF(50, 15), new PointF(1445, 15));

                    for (int i = 0; i < numberOfLines; i++)
                    {
                        ctx.DrawLines(grayPen, new PointF(50 + (i * width), 15), new PointF(50 + (i * width), 115));
                    }

                    for (int i = 1; i < historicCoverages.Count; i++)
                    {
                        float x1 = 50 + ((i - 1) * width);
                        float y1 = 15 + (((100 - (float)historicCoverages[i - 1].CoverageQuota.GetValueOrDefault()) * totalHeight) / 100);

                        float x2 = 50 + (i * width);
                        float y2 = 15 + (((100 - (float)historicCoverages[i].CoverageQuota.GetValueOrDefault()) * totalHeight) / 100);

                        ctx.DrawLines(redPen, new PointF(x1, y1), new PointF(x2, y2));
                    }

                    if (historicCoverages.Any(h => h.BranchCoverageQuota.HasValue))
                    {
                        for (int i = 1; i < historicCoverages.Count; i++)
                        {
                            float x1 = 50 + ((i - 1) * width);
                            float y1 = 15 + (((100 - (float)historicCoverages[i - 1].BranchCoverageQuota.GetValueOrDefault()) * totalHeight) / 100);

                            float x2 = 50 + (i * width);
                            float y2 = 15 + (((100 - (float)historicCoverages[i].BranchCoverageQuota.GetValueOrDefault()) * totalHeight) / 100);

                            ctx.DrawLines(bluePen, new PointF(x1, y1), new PointF(x2, y2));
                        }
                    }

                    for (int i = 0; i < historicCoverages.Count; i++)
                    {
                        float x1 = 50 + (i * width);
                        float y1 = 15 + (((100 - (float)historicCoverages[i].CoverageQuota.GetValueOrDefault()) * totalHeight) / 100);

                        ctx.Fill(redBrush, new EllipsePolygon(x1, y1, 3));
                    }

                    if (historicCoverages.Any(h => h.BranchCoverageQuota.HasValue))
                    {
                        for (int i = 0; i < historicCoverages.Count; i++)
                        {
                            float x1 = 50 + (i * width);
                            float y1 = 15 + (((100 - (float)historicCoverages[i].BranchCoverageQuota.GetValueOrDefault()) * totalHeight) / 100);

                            ctx.Fill(blueBrush, new EllipsePolygon(x1, y1, 3));
                        }
                    }

                    try
                    {
                        var font = SystemFonts.CreateFont("Arial", 11, FontStyle.Regular);
                        var drawingOptions = new DrawingOptions();
                        drawingOptions.TextOptions.HorizontalAlignment = HorizontalAlignment.Right;
                        ctx.DrawText(drawingOptions, "100", font, Color.Gray, new PointF(38, 5));
                        ctx.DrawText(drawingOptions, "75", font, Color.Gray, new PointF(38, 30));
                        ctx.DrawText(drawingOptions, "50", font, Color.Gray, new PointF(38, 55));
                        ctx.DrawText(drawingOptions, "25", font, Color.Gray, new PointF(38, 80));
                        ctx.DrawText(drawingOptions, "0", font, Color.Gray, new PointF(38, 105));
                    }
                    catch (SixLabors.Fonts.Exceptions.FontFamilyNotFoundException)
                    {
                        // Font 'Arial' may not be present on Linux
                    }
                });

                image.Save(output, new PngEncoder());
                return output.ToArray();
            }
        }
    }
}
