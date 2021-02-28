using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Web;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Response;
using ChartDirector;

namespace Oasis.Indicators
{
    public class ImageCreator
    {
        public static void ExtendLine(Vector2 p1, Vector2 p2, float distance, out Vector2 start, out Vector2 end)
        {
            Vector2 direction = p2 - p1;
            direction = Vector2.Normalize(direction);
            Vector2 offset = direction * distance;
            start = p1 - offset;
            end = p2 + offset;
        }

        public static byte[] Graph(string pair, KlineInterval interval, List<Trendline> trendlines,
            List<decimal> smaPoints, List<decimal> volume, List<Resistance> resistances)
        {
            CandleData data = DataManager.GetCandleDataForPairAndInterval(pair, interval);

            List<double> highs = new List<double>();
            List<double> lows = new List<double>();
            List<double> close = new List<double>();
            List<double> open = new List<double>();
            List<DateTime> times = new List<DateTime>();

            var candles = new List<KlineCandleStickResponse>(data.candles);

            for (int i = candles.Count - 100; i < candles.Count; i++)
            {
                KlineCandleStickResponse candle = candles[i];
                highs.Add(Convert.ToDouble(candle.High));
                lows.Add(Convert.ToDouble(candle.Low));
                close.Add(Convert.ToDouble(candle.Close));
                open.Add(Convert.ToDouble(candle.Open));
                times.Add(candle.OpenTime);
            }

            for (int i = 0; i < 5; i++)
            {
                highs.Add(Convert.ToDouble(candles[candles.Count - 1].Close));
                lows.Add(Convert.ToDouble(candles[candles.Count - 1].Close));
                close.Add(Convert.ToDouble(candles[candles.Count - 1].Close));
                open.Add(Convert.ToDouble(candles[candles.Count - 1].Close));
            }

            List<double> volumes = new List<double>();

            foreach (var c in volume)
            {
                volumes.Add(Convert.ToDouble(c));
            }


            var xyChart = new ChartDirector.FinanceChart(1920);
            xyChart.setPlotAreaStyle(0x171b26, 0x232632, 0x232632, 0x232632, 0x232632);
            xyChart.setData(times.ToArray(), highs.ToArray(), lows.ToArray(), open.ToArray(), close.ToArray(),
                volumes.ToArray(), 0);
            var chart = xyChart.addMainChart(1080);
            chart.xAxis().setLabelStyle("Arial", 8, 0xffffff, 0);
            chart.yAxis().setLabelStyle("Arial", 8, 0xffffff, 0);
            
            xyChart.setBackground(0x171b26);
            
            //xyChart.getChart().setColor(Chart.TextColor, 0xFFFFFF);
            
            //xyChart.getChart().setBackground(0x171b26);
            //xyChart.setPlotArea(50, 25, 1850, 1000).setGridColor(0x232632, 0x232632);
            xyChart.addTitle("TA Trading Bot of Pair " + pair + " and Timeframe " +
                             Enum.GetName(typeof(KlineInterval), interval), "bold", 10, 0xffffff);
            //xyChart.addBarLayer(volumes.ToArray()).setBaseLine(Convert.ToDouble(candles[candles.Count - 1].Close - candles[candles.Count - 1].Close / 10m));
            //var axies = xyChart.addAxis(4, xyChart.getYCoor(35000));
            //xyChart.addBarLayer(volumes.ToArray()).setUseYAxis(axies);

            /*chart.setBackground(0x171b26);
            chart.setColor(Chart.TextColor, 0x000000);
            */
            var layer = xyChart.addCandleStick(0x00ff00, 0xff0000);
            //layer.setExtraColors(0xffffff, 0xffffff, 0xffffff, 0xffffff);
            layer.setColors(0x00ff00, 0x00ff00,0xff0000,0xff0000 );

            layer.setLineWidth(2);

            List<double> smaData = new List<double>();

            foreach (var c in smaPoints)
            {
                smaData.Add(Convert.ToDouble(c));
            }

            //xyChart.addVolBars(200, 0x00ff00, 0xff0000, 0x808080);
            //xyChart.addRSI(200, 14, 0xff0000, 80, 0x00ff00, 0xff0000);
            xyChart.addSimpleMovingAvg(14, 0xd8a704);
            //xyChart.addLineLayer(smaData.ToArray(), 0xd8a704).setLineWidth(6);

            xyChart.layout();

            foreach (var tl in trendlines)
            {
                Vector2 start, end;

                ExtendLine(
                    new Vector2(chart.getXCoor((tl.startpoint - 400)),
                        chart.getYCoor(Convert.ToDouble(tl.linePoints[0].value))),
                    new Vector2(chart.getXCoor(tl.linePoints[tl.linePoints.Count - 1].candle - 400),
                        chart.getYCoor(Convert.ToDouble(tl.linePoints[tl.linePoints.Count - 1].value))),
                    400f, out start, out end
                );
                
                xyChart.addLine(Convert.ToInt32(start.X), Convert.ToInt32(start.Y), Convert.ToInt32(end.X),
                    Convert.ToInt32(end.Y), 0xFFC300).setWidth(4);

            }

            foreach (var resistance in resistances)
            {
                var y = Convert.ToDouble(resistance.Value);
                xyChart.addLine(chart.getXCoor(0), chart.getYCoor(y), chart.getXCoor(100), chart.getYCoor(y), 0x00ff00)
                    .setWidth(3);
            }

            xyChart.makeChart(pair + "_" + Enum.GetName(typeof(KlineInterval), interval) + ".jpg");
            return xyChart.makeChart2(0);
        }

        public static void CreateChart()
        {
            using (Image dest = new Bitmap(1920, 1080))
            {
                Graphics graph = Graphics.FromImage(dest);
                graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graph.Clear(Color.White);
                graph.DrawLine(Pens.Black, new Point(20, 960), new Point(1900, 960) );
                dest.Save("test.jpg");
            }
        }
    }
}