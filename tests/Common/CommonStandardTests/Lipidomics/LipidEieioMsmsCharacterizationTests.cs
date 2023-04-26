﻿using CompMs.Common.Components;
using CompMs.Common.DataObj.Property;
using CompMs.Common.DataObj.Result;
using CompMs.Common.Enum;
using CompMs.Common.Parameter;
using CompMs.Common.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompMs.Common.Lipidomics.Tests
{
    [TestClass()]
    public class LipidEieioMsmsCharacterizationTests
    {
        private AdductIon adduct;

        [TestMethod()]
        public void CarCharacterizationTest()
        {
            // CAR 18:1[M+H]+
            var target = new MSScanProperty
            {
                PrecursorMz = 426.36762,
                Spectrum = new List<SpectrumPeak>
                {
                    new SpectrumPeak { Mass = 111.11671, Intensity =23, },
                    new SpectrumPeak { Mass = 144.10157, Intensity =160, },
                    new SpectrumPeak { Mass = 157.04702, Intensity =32, },
                    new SpectrumPeak { Mass = 160.09647, Intensity =24, },
                    new SpectrumPeak { Mass = 162.11163, Intensity =74, },
                    new SpectrumPeak { Mass = 165.1268, Intensity =20, },
                    new SpectrumPeak { Mass = 172.13265, Intensity =41, },
                    new SpectrumPeak { Mass = 179.14242, Intensity =41, },
                    new SpectrumPeak { Mass = 186.1502, Intensity =20, },
                    new SpectrumPeak { Mass = 195.17162, Intensity =25, },
                    new SpectrumPeak { Mass = 198.16096, Intensity =20, },
                    new SpectrumPeak { Mass = 199.09525, Intensity =35, },
                    new SpectrumPeak { Mass = 202.10876, Intensity =34, },
                    new SpectrumPeak { Mass = 203.11718, Intensity =493, },
                    new SpectrumPeak { Mass = 204.12297, Intensity =76, },
                    new SpectrumPeak { Mass = 213.11257, Intensity =62, },
                    new SpectrumPeak { Mass = 213.17622, Intensity =21, },
                    new SpectrumPeak { Mass = 216.1246, Intensity =1876, },
                    new SpectrumPeak { Mass = 217.13132, Intensity =64, },
                    new SpectrumPeak { Mass = 223.1702, Intensity =23, },
                    new SpectrumPeak { Mass = 227.12663, Intensity =53, },
                    new SpectrumPeak { Mass = 228.12219, Intensity =24, },
                    new SpectrumPeak { Mass = 228.19294, Intensity =34, },
                    new SpectrumPeak { Mass = 230.1406, Intensity =470, },
                    new SpectrumPeak { Mass = 231.14627, Intensity =23, },
                    new SpectrumPeak { Mass = 237.18604, Intensity =25, },
                    new SpectrumPeak { Mass = 239.12527, Intensity =32, },
                    new SpectrumPeak { Mass = 241.14421, Intensity =40, },
                    new SpectrumPeak { Mass = 244.1551, Intensity =138, },
                    new SpectrumPeak { Mass = 245.16103, Intensity =54, },
                    new SpectrumPeak { Mass = 247.24146, Intensity =50, },
                    new SpectrumPeak { Mass = 255.16038, Intensity =64, },
                    new SpectrumPeak { Mass = 256.22767, Intensity =38, },
                    new SpectrumPeak { Mass = 258.17069, Intensity =415, },
                    new SpectrumPeak { Mass = 259.1776, Intensity =81, },
                    new SpectrumPeak { Mass = 263.23713, Intensity =22, },
                    new SpectrumPeak { Mass = 264.24207, Intensity =29, },
                    new SpectrumPeak { Mass = 265.25281, Intensity =208, },
                    new SpectrumPeak { Mass = 267.15813, Intensity =35, },
                    new SpectrumPeak { Mass = 269.17397, Intensity =79, },
                    new SpectrumPeak { Mass = 270.16759, Intensity =23, },
                    new SpectrumPeak { Mass = 271.17688, Intensity =28, },
                    new SpectrumPeak { Mass = 272.1874, Intensity =786, },
                    new SpectrumPeak { Mass = 273.19361, Intensity =116, },
                    new SpectrumPeak { Mass = 274.19998, Intensity =29, },
                    new SpectrumPeak { Mass = 281.17504, Intensity =20, },
                    new SpectrumPeak { Mass = 282.24086, Intensity =39, },
                    new SpectrumPeak { Mass = 283.18941, Intensity =43, },
                    new SpectrumPeak { Mass = 283.24909, Intensity =32, },
                    new SpectrumPeak { Mass = 286.20137, Intensity =267, },
                    new SpectrumPeak { Mass = 287.20864, Intensity =160, },
                    new SpectrumPeak { Mass = 296.25629, Intensity =31, },
                    new SpectrumPeak { Mass = 297.20677, Intensity =45, },
                    new SpectrumPeak { Mass = 298.20106, Intensity =96, },
                    new SpectrumPeak { Mass = 300.21744, Intensity =345, },
                    new SpectrumPeak { Mass = 301.22561, Intensity =129, },
                    new SpectrumPeak { Mass = 311.21543, Intensity =31, },
                    new SpectrumPeak { Mass = 312.21803, Intensity =203, },
                    new SpectrumPeak { Mass = 313.22498, Intensity =65, },
                    new SpectrumPeak { Mass = 314.23486, Intensity =446, },
                    new SpectrumPeak { Mass = 315.24069, Intensity =127, },
                    new SpectrumPeak { Mass = 321.27928, Intensity =77, },
                    new SpectrumPeak { Mass = 324.28722, Intensity =21, },
                    new SpectrumPeak { Mass = 325.22659, Intensity =27, },
                    new SpectrumPeak { Mass = 326.23271, Intensity =501, },
                    new SpectrumPeak { Mass = 327.24329, Intensity =629, },
                    new SpectrumPeak { Mass = 328.25062, Intensity =974, },
                    new SpectrumPeak { Mass = 329.25738, Intensity =204, },
                    new SpectrumPeak { Mass = 330.2668, Intensity =26, },
                    new SpectrumPeak { Mass = 338.34618, Intensity =23, },
                    new SpectrumPeak { Mass = 339.24178, Intensity =23, },
                    new SpectrumPeak { Mass = 340.24813, Intensity =515, },
                    new SpectrumPeak { Mass = 341.25883, Intensity =120, },
                    new SpectrumPeak { Mass = 342.2672, Intensity =1141, },
                    new SpectrumPeak { Mass = 343.27409, Intensity =172, },
                    new SpectrumPeak { Mass = 344.27771, Intensity =26, },
                    new SpectrumPeak { Mass = 354.26627, Intensity =203, },
                    new SpectrumPeak { Mass = 355.27546, Intensity =108, },
                    new SpectrumPeak { Mass = 356.28104, Intensity =285, },
                    new SpectrumPeak { Mass = 366.33265, Intensity =42, },
                    new SpectrumPeak { Mass = 367.28408, Intensity =122, },
                    new SpectrumPeak { Mass = 368.28013, Intensity =162, },
                    new SpectrumPeak { Mass = 369.2872, Intensity =55, },
                    new SpectrumPeak { Mass = 370.29826, Intensity =182, },
                    new SpectrumPeak { Mass = 379.34715, Intensity =32, },
                    new SpectrumPeak { Mass = 380.35376, Intensity =159, },
                    new SpectrumPeak { Mass = 381.36351, Intensity =1369, },
                    new SpectrumPeak { Mass = 382.29576, Intensity =220, },
                    new SpectrumPeak { Mass = 383.30415, Intensity =87, },
                    new SpectrumPeak { Mass = 396.31238, Intensity =150, },
                    new SpectrumPeak { Mass = 397.32018, Intensity =191, },
                    new SpectrumPeak { Mass = 398.36118, Intensity =48, },
                    new SpectrumPeak { Mass = 407.3405, Intensity =20, },
                    new SpectrumPeak { Mass = 408.35039, Intensity =56, },
                    new SpectrumPeak { Mass = 410.33014, Intensity =217, },
                    new SpectrumPeak { Mass = 411.33546, Intensity =72, },
                    new SpectrumPeak { Mass = 423.33533, Intensity =163, },
                    new SpectrumPeak { Mass = 424.34539, Intensity =1319, },
                    new SpectrumPeak { Mass = 424.95006, Intensity =73, },
                    new SpectrumPeak { Mass = 425.35171, Intensity =6541, },
                    new SpectrumPeak { Mass = 426.36762, Intensity =28554, },
                    new SpectrumPeak { Mass = 427.85577, Intensity =21, },
                    new SpectrumPeak { Mass = 428.28133, Intensity =20, },
                    new SpectrumPeak { Mass = 429.33362, Intensity =20, },
                    new SpectrumPeak { Mass = 429.74857, Intensity =21, },
                    new SpectrumPeak { Mass = 430.7242, Intensity =20, },
                }
            };
            var result = LipidEieioMsmsCharacterization.JudgeIfAcylcarnitine(target, 0.01, 426.3578f, 18, 1, adduct = new AdductIon() { AdductIonName = "[M+H]+"});
            Console.WriteLine($"{result.LipidName}");

        }
        [TestMethod()]
        public void EtherPECharacterizationTest()
        {
            //PE O-16:0/18:1
            var target = new MSScanProperty
            {
                PrecursorMz = 704.55634,
                Spectrum = new List<SpectrumPeak>
                {
                    new SpectrumPeak { Mass = 109.10044, Intensity =5.3239, },
                    new SpectrumPeak { Mass = 124.01589, Intensity =74.4659, },
                    new SpectrumPeak { Mass = 129.06211, Intensity =8.0896, },
                    new SpectrumPeak { Mass = 133.10131, Intensity =7.4673, },
                    new SpectrumPeak { Mass = 134.10262, Intensity =5.0474, },
                    new SpectrumPeak { Mass = 134.98373, Intensity =32.5659, },
                    new SpectrumPeak { Mass = 135.08049, Intensity =7.7439, },
                    new SpectrumPeak { Mass = 135.11624, Intensity =18.6683, },
                    new SpectrumPeak { Mass = 136.01689, Intensity =6.4993, },
                    new SpectrumPeak { Mass = 137.09486, Intensity =11.9616, },
                    new SpectrumPeak { Mass = 138.1267, Intensity =5.9462, },
                    new SpectrumPeak { Mass = 139.03065, Intensity =7.329, },
                    new SpectrumPeak { Mass = 139.11104, Intensity =11.0627, },
                    new SpectrumPeak { Mass = 140.00797, Intensity =5.7388, },
                    new SpectrumPeak { Mass = 140.99477, Intensity =16.9398, },
                    new SpectrumPeak { Mass = 142.0296, Intensity =234.8752, },
                    new SpectrumPeak { Mass = 143.74088, Intensity =5.8771, },
                    new SpectrumPeak { Mass = 147.11505, Intensity =9.1267, },
                    new SpectrumPeak { Mass = 149.13132, Intensity =17.3546, },
                    new SpectrumPeak { Mass = 150.09113, Intensity =5.2548, },
                    new SpectrumPeak { Mass = 151.11273, Intensity =5.8079, },
                    new SpectrumPeak { Mass = 151.14578, Intensity =6.5685, },
                    new SpectrumPeak { Mass = 152.1414, Intensity =21.7106, },
                    new SpectrumPeak { Mass = 153.12533, Intensity =9.0576, },
                    new SpectrumPeak { Mass = 154.02447, Intensity =47.2931, },
                    new SpectrumPeak { Mass = 155.01417, Intensity =29.9385, },
                    new SpectrumPeak { Mass = 157.02403, Intensity =19.3597, },
                    new SpectrumPeak { Mass = 159.11557, Intensity =7.1908, },
                    new SpectrumPeak { Mass = 163.146, Intensity =13.4827, },
                    new SpectrumPeak { Mass = 166.02628, Intensity =120.0996, },
                    new SpectrumPeak { Mass = 167.1507, Intensity =10.5787, },
                    new SpectrumPeak { Mass = 167.16685, Intensity =8.7119, },
                    new SpectrumPeak { Mass = 168.17405, Intensity =26.6197, },
                    new SpectrumPeak { Mass = 169.12146, Intensity =15.4878, },
                    new SpectrumPeak { Mass = 169.15186, Intensity =7.5365, },
                    new SpectrumPeak { Mass = 173.0192, Intensity =45.4263, },
                    new SpectrumPeak { Mass = 173.79276, Intensity =22.3328, },
                    new SpectrumPeak { Mass = 177.16322, Intensity =5.6005, },
                    new SpectrumPeak { Mass = 179.03298, Intensity =19.5672, },
                    new SpectrumPeak { Mass = 180.04091, Intensity =82.2789, },
                    new SpectrumPeak { Mass = 181.04778, Intensity =5.7388, },
                    new SpectrumPeak { Mass = 181.17459, Intensity =8.8502, },
                    new SpectrumPeak { Mass = 182.05933, Intensity =150.7294, },
                    new SpectrumPeak { Mass = 183.44896, Intensity =7.4673, },
                    new SpectrumPeak { Mass = 184.0408, Intensity =214.4783, },
                    new SpectrumPeak { Mass = 185.11514, Intensity =5.1856, },
                    new SpectrumPeak { Mass = 188.1609, Intensity =8.9885, },
                    new SpectrumPeak { Mass = 192.13817, Intensity =8.297, },
                    new SpectrumPeak { Mass = 192.18659, Intensity =5.8771, },
                    new SpectrumPeak { Mass = 193.15566, Intensity =5.8771, },
                    new SpectrumPeak { Mass = 194.15262, Intensity =5.1856, },
                    new SpectrumPeak { Mass = 196.03631, Intensity =31.4596, },
                    new SpectrumPeak { Mass = 197.04097, Intensity =7.3982, },
                    new SpectrumPeak { Mass = 198.05245, Intensity =19.1523, },
                    new SpectrumPeak { Mass = 199.06039, Intensity =6.0153, },
                    new SpectrumPeak { Mass = 200.06663, Intensity =8.6427, },
                    new SpectrumPeak { Mass = 209.17455, Intensity =8.2279, },
                    new SpectrumPeak { Mass = 212.07046, Intensity =9.5416, },
                    new SpectrumPeak { Mass = 212.16175, Intensity =14.0358, },
                    new SpectrumPeak { Mass = 218.20057, Intensity =12.0307, },
                    new SpectrumPeak { Mass = 221.1852, Intensity =5.3239, },
                    new SpectrumPeak { Mass = 222.18992, Intensity =5.7388, },
                    new SpectrumPeak { Mass = 223.19558, Intensity =4.9782, },
                    new SpectrumPeak { Mass = 226.16024, Intensity =7.4673, },
                    new SpectrumPeak { Mass = 226.19424, Intensity =8.9193, },
                    new SpectrumPeak { Mass = 239.09354, Intensity =14.105, },
                    new SpectrumPeak { Mass = 239.12508, Intensity =8.8502, },
                    new SpectrumPeak { Mass = 241.18045, Intensity =11.201, },
                    new SpectrumPeak { Mass = 246.23348, Intensity =23.0243, },
                    new SpectrumPeak { Mass = 247.23998, Intensity =5.7388, },
                    new SpectrumPeak { Mass = 251.23572, Intensity =5.7388, },
                    new SpectrumPeak { Mass = 253.10209, Intensity =10.6479, },
                    new SpectrumPeak { Mass = 253.2103, Intensity =14.589, },
                    new SpectrumPeak { Mass = 253.24753, Intensity =12.169, },
                    new SpectrumPeak { Mass = 262.23943, Intensity =5.8771, },
                    new SpectrumPeak { Mass = 263.239, Intensity =9.6799, },
                    new SpectrumPeak { Mass = 264.24449, Intensity =10.8553, },
                    new SpectrumPeak { Mass = 265.24431, Intensity =21.918, },
                    new SpectrumPeak { Mass = 266.74476, Intensity =5.8079, },
                    new SpectrumPeak { Mass = 268.12732, Intensity =8.9885, },
                    new SpectrumPeak { Mass = 279.26309, Intensity =11.2701, },
                    new SpectrumPeak { Mass = 280.25601, Intensity =7.5365, },
                    new SpectrumPeak { Mass = 280.27618, Intensity =6.7068, },
                    new SpectrumPeak { Mass = 280.76059, Intensity =6.3611, },
                    new SpectrumPeak { Mass = 281.25473, Intensity =5.6005, },
                    new SpectrumPeak { Mass = 281.28853, Intensity =16.3175, },
                    new SpectrumPeak { Mass = 281.7699, Intensity =6.1536, },
                    new SpectrumPeak { Mass = 292.1322, Intensity =7.6056, },
                    new SpectrumPeak { Mass = 293.27664, Intensity =11.5467, },
                    new SpectrumPeak { Mass = 294.14706, Intensity =5.0474, },
                    new SpectrumPeak { Mass = 296.28829, Intensity =8.5045, },
                    new SpectrumPeak { Mass = 297.27316, Intensity =19.9129, },
                    new SpectrumPeak { Mass = 298.27506, Intensity =7.329, },
                    new SpectrumPeak { Mass = 299.29336, Intensity =12.5147, },
                    new SpectrumPeak { Mass = 306.27841, Intensity =18.5992, },
                    new SpectrumPeak { Mass = 307.28067, Intensity =7.7439, },
                    new SpectrumPeak { Mass = 308.29671, Intensity =123.9715, },
                    new SpectrumPeak { Mass = 309.16926, Intensity =13.621, },
                    new SpectrumPeak { Mass = 310.14169, Intensity =15.3495, },
                    new SpectrumPeak { Mass = 311.2561, Intensity =10.9244, },
                    new SpectrumPeak { Mass = 316.73533, Intensity =8.1587, },
                    new SpectrumPeak { Mass = 321.274, Intensity =20.1894, },
                    new SpectrumPeak { Mass = 322.28909, Intensity =19.7746, },
                    new SpectrumPeak { Mass = 323.25255, Intensity =9.6799, },
                    new SpectrumPeak { Mass = 323.28855, Intensity =8.8502, },
                    new SpectrumPeak { Mass = 323.74816, Intensity =7.0525, },
                    new SpectrumPeak { Mass = 324.27899, Intensity =10.0947, },
                    new SpectrumPeak { Mass = 324.32583, Intensity =5.0474, },
                    new SpectrumPeak { Mass = 325.2762, Intensity =5.8079, },
                    new SpectrumPeak { Mass = 326.30644, Intensity =18.7375, },
                    new SpectrumPeak { Mass = 330.74949, Intensity =7.1216, },
                    new SpectrumPeak { Mass = 331.6969, Intensity =14.589, },
                    new SpectrumPeak { Mass = 335.18884, Intensity =5.8771, },
                    new SpectrumPeak { Mass = 336.27012, Intensity =13.2753, },
                    new SpectrumPeak { Mass = 337.27296, Intensity =5.8079, },
                    new SpectrumPeak { Mass = 337.75903, Intensity =9.4033, },
                    new SpectrumPeak { Mass = 338.27341, Intensity =7.6748, },
                    new SpectrumPeak { Mass = 339.29196, Intensity =17.8386, },
                    new SpectrumPeak { Mass = 341.30812, Intensity =9.1959, },
                    new SpectrumPeak { Mass = 347.23391, Intensity =5.3239, },
                    new SpectrumPeak { Mass = 351.22833, Intensity =5.3931, },
                    new SpectrumPeak { Mass = 351.28451, Intensity =8.2279, },
                    new SpectrumPeak { Mass = 351.77683, Intensity =6.0153, },
                    new SpectrumPeak { Mass = 352.28016, Intensity =117.8179, },
                    new SpectrumPeak { Mass = 353.30726, Intensity =61.3981, },
                    new SpectrumPeak { Mass = 365.23988, Intensity =10.717, },
                    new SpectrumPeak { Mass = 366.30757, Intensity =13.2061, },
                    new SpectrumPeak { Mass = 367.29887, Intensity =6.7759, },
                    new SpectrumPeak { Mass = 367.31766, Intensity =13.3444, },
                    new SpectrumPeak { Mass = 370.25835, Intensity =10.233, },
                    new SpectrumPeak { Mass = 379.2532, Intensity =8.0205, },
                    new SpectrumPeak { Mass = 379.26505, Intensity =10.8553, },
                    new SpectrumPeak { Mass = 379.28301, Intensity =6.7068, },
                    new SpectrumPeak { Mass = 379.33143, Intensity =6.7759, },
                    new SpectrumPeak { Mass = 381.3376, Intensity =5.8771, },
                    new SpectrumPeak { Mass = 393.33229, Intensity =6.2919, },
                    new SpectrumPeak { Mass = 394.34279, Intensity =12.9987, },
                    new SpectrumPeak { Mass = 395.34925, Intensity =16.0409, },
                    new SpectrumPeak { Mass = 396.31658, Intensity =8.297, },
                    new SpectrumPeak { Mass = 397.27117, Intensity =70.1099, },
                    new SpectrumPeak { Mass = 405.2642, Intensity =13.4135, },
                    new SpectrumPeak { Mass = 406.273, Intensity =74.051, },
                    new SpectrumPeak { Mass = 407.35713, Intensity =13.7592, },
                    new SpectrumPeak { Mass = 408.35326, Intensity =8.4353, },
                    new SpectrumPeak { Mass = 409.36569, Intensity =111.1111, },
                    new SpectrumPeak { Mass = 420.28925, Intensity =14.3815, },
                    new SpectrumPeak { Mass = 420.32393, Intensity =21.2957, },
                    new SpectrumPeak { Mass = 421.29997, Intensity =303.2566, },
                    new SpectrumPeak { Mass = 422.30489, Intensity =37.8898, },
                    new SpectrumPeak { Mass = 423.31599, Intensity =10.1639, },
                    new SpectrumPeak { Mass = 423.36454, Intensity =11.9616, },
                    new SpectrumPeak { Mass = 435.38561, Intensity =16.4558, },
                    new SpectrumPeak { Mass = 437.26233, Intensity =17.3546, },
                    new SpectrumPeak { Mass = 437.38846, Intensity =11.201, },
                    new SpectrumPeak { Mass = 438.30005, Intensity =18.7375, },
                    new SpectrumPeak { Mass = 438.36148, Intensity =8.5736, },
                    new SpectrumPeak { Mass = 438.40575, Intensity =7.5365, },
                    new SpectrumPeak { Mass = 439.61086, Intensity =6.7068, },
                    new SpectrumPeak { Mass = 440.31571, Intensity =32.2893, },
                    new SpectrumPeak { Mass = 446.33089, Intensity =11.3393, },
                    new SpectrumPeak { Mass = 447.38249, Intensity =6.5685, },
                    new SpectrumPeak { Mass = 449.40246, Intensity =21.0192, },
                    new SpectrumPeak { Mass = 451.41642, Intensity =16.4558, },
                    new SpectrumPeak { Mass = 452.41986, Intensity =13.137, },
                    new SpectrumPeak { Mass = 461.39869, Intensity =5.6005, },
                    new SpectrumPeak { Mass = 462.34323, Intensity =6.3611, },
                    new SpectrumPeak { Mass = 463.30559, Intensity =79.375, },
                    new SpectrumPeak { Mass = 464.42096, Intensity =21.918, },
                    new SpectrumPeak { Mass = 465.43027, Intensity =14.9347, },
                    new SpectrumPeak { Mass = 466.43731, Intensity =44.5966, },
                    new SpectrumPeak { Mass = 477.42126, Intensity =19.7746, },
                    new SpectrumPeak { Mass = 478.2928, Intensity =12.9295, },
                    new SpectrumPeak { Mass = 478.43818, Intensity =6.8451, },
                    new SpectrumPeak { Mass = 479.44648, Intensity =9.1267, },
                    new SpectrumPeak { Mass = 480.41881, Intensity =6.4302, },
                    new SpectrumPeak { Mass = 480.45714, Intensity =6.9833, },
                    new SpectrumPeak { Mass = 491.43153, Intensity =7.5365, },
                    new SpectrumPeak { Mass = 492.44426, Intensity =6.4993, },
                    new SpectrumPeak { Mass = 494.32966, Intensity =12.9295, },
                    new SpectrumPeak { Mass = 505.46244, Intensity =8.9193, },
                    new SpectrumPeak { Mass = 506.32005, Intensity =5.5314, },
                    new SpectrumPeak { Mass = 519.47451, Intensity =14.7272, },
                    new SpectrumPeak { Mass = 520.48362, Intensity =8.5045, },
                    new SpectrumPeak { Mass = 523.36093, Intensity =9.4033, },
                    new SpectrumPeak { Mass = 533.48919, Intensity =16.0409, },
                    new SpectrumPeak { Mass = 534.50559, Intensity =5.6696, },
                    new SpectrumPeak { Mass = 550.38525, Intensity =11.6158, },
                    new SpectrumPeak { Mass = 559.5133, Intensity =14.3815, },
                    new SpectrumPeak { Mass = 560.51357, Intensity =38.3046, },
                    new SpectrumPeak { Mass = 561.53197, Intensity =267.9942, },
                    new SpectrumPeak { Mass = 562.53591, Intensity =307.0594, },
                    new SpectrumPeak { Mass = 563.54075, Intensity =225.9559, },
                    new SpectrumPeak { Mass = 574.39541, Intensity =5.6005, },
                    new SpectrumPeak { Mass = 575.40217, Intensity =7.0525, },
                    new SpectrumPeak { Mass = 576.39985, Intensity =30.5607, },
                    new SpectrumPeak { Mass = 578.42385, Intensity =7.6748, },
                    new SpectrumPeak { Mass = 589.41079, Intensity =11.1319, },
                    new SpectrumPeak { Mass = 590.41755, Intensity =75.5721, },
                    new SpectrumPeak { Mass = 591.43161, Intensity =12.9295, },
                    new SpectrumPeak { Mass = 592.43593, Intensity =6.2228, },
                    new SpectrumPeak { Mass = 594.51728, Intensity =5.2548, },
                    new SpectrumPeak { Mass = 605.44757, Intensity =29.8693, },
                    new SpectrumPeak { Mass = 606.58497, Intensity =6.9833, },
                    new SpectrumPeak { Mass = 607.45648, Intensity =5.3931, },
                    new SpectrumPeak { Mass = 620.46492, Intensity =7.8822, },
                    new SpectrumPeak { Mass = 625.47706, Intensity =6.3611, },
                    new SpectrumPeak { Mass = 633.46761, Intensity =6.2228, },
                    new SpectrumPeak { Mass = 647.48347, Intensity =6.0845, },
                    new SpectrumPeak { Mass = 658.47577, Intensity =7.8822, },
                    new SpectrumPeak { Mass = 660.49855, Intensity =7.1908, },
                    new SpectrumPeak { Mass = 661.50543, Intensity =4.9782, },
                    new SpectrumPeak { Mass = 662.5148, Intensity =4.9782, },
                    new SpectrumPeak { Mass = 675.52547, Intensity =7.6056, },
                    new SpectrumPeak { Mass = 687.49648, Intensity =5.6005, },
                    new SpectrumPeak { Mass = 701.53308, Intensity =16.9398, },
                    new SpectrumPeak { Mass = 702.54708, Intensity =76.6093, },
                    new SpectrumPeak { Mass = 703.55261, Intensity =486.6902, },
                    new SpectrumPeak { Mass = 704.55308, Intensity =1000, },
                    new SpectrumPeak { Mass = 706.47312, Intensity =36.7144, },
                    new SpectrumPeak { Mass = 706.60477, Intensity =17.2163, },
                }
            };
            var result = LipidEieioMsmsCharacterization.JudgeIfEtherpe(target, 0.01, 704.5589f, 34, 1, 14, 22, 0, 1, adduct = new AdductIon() { AdductIonName = "[M+H]+" });
            Console.WriteLine($"{result.LipidName}");

        }
    }
}