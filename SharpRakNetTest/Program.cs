﻿using SharpRakNet;
using SharpRakNet.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SharpRakNetTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RaknetListener listener = new RaknetListener(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 19132));
            listener.SessionConnected += OnSessionEstablished;
            listener.BeginListener();
        }

        static void OnSessionEstablished(RaknetSession session)
        {
            Console.WriteLine("OnSessionEstablished");
            session.SessionDisconnected += OnDisconnected;
            session.SessionReceive += OnReceive;
            session.Sendq.Insert(Reliability.ReliableOrdered, new byte[] { 1, 2, 3 });
        }

        static void OnDisconnected(RaknetSession session)
        {
            Console.WriteLine(session.PeerEndPoint);
        }

        static void OnReceive(byte[] buf)
        {
            Console.WriteLine("Length", buf.Length);
            //PrintBytes(buf);
        }

        public static void PrintBytes(byte[] byteArray)
        {
            Console.Write("[");
            foreach (byte b in byteArray)
            {
                Console.Write(b + " ");
            }
            Console.Write("]\n");
        }

        static bool TestClientPacket1()
        {
            var p0 = new byte[] 
            {
        132, 0, 0, 0, 64, 0, 144, 0, 0, 0, 9, 162, 70, 235, 28, 218, 182, 26, 192, 0, 0, 0, 0, 16,
        151, 43, 113, 0,
            };
            var p1 = new byte[]
            {
        132, 1, 0, 0, 64, 0, 144, 0, 0, 0, 9, 162, 70, 235, 28, 218, 182, 26, 192, 0, 0, 0, 0, 16,
        151, 43, 113, 0,
            };
            var p2 = new byte[] 
            {
        132, 2, 0, 0, 96, 9, 64, 1, 0, 0, 0, 0, 0, 0, 19, 4, 83, 237, 234, 82, 74, 188, 6, 23, 0,
        225, 138, 0, 0, 0, 0, 254, 128, 0, 0, 0, 0, 0, 0, 196, 178, 112, 86, 5, 59, 97, 219, 15, 0,
        0, 0, 6, 23, 0, 225, 138, 0, 0, 0, 0, 254, 128, 0, 0, 0, 0, 0, 0, 188, 210, 59, 150, 246,
        167, 182, 213, 33, 0, 0, 0, 6, 23, 0, 225, 138, 0, 0, 0, 0, 254, 128, 0, 0, 0, 0, 0, 0,
        132, 194, 47, 23, 175, 46, 78, 138, 23, 0, 0, 0, 6, 23, 0, 225, 138, 0, 0, 0, 0, 254, 128,
        0, 0, 0, 0, 0, 0, 136, 219, 85, 240, 191, 125, 172, 233, 10, 0, 0, 0, 6, 23, 0, 225, 138,
        0, 0, 0, 0, 254, 128, 0, 0, 0, 0, 0, 0, 80, 211, 212, 44, 191, 227, 124, 40, 13, 0, 0, 0,
        6, 23, 0, 225, 138, 0, 0, 0, 0, 254, 128, 0, 0, 0, 0, 0, 0, 77, 13, 19, 149, 102, 140, 134,
        77, 16, 0, 0, 0, 4, 83, 237, 239, 254, 225, 138, 4, 63, 87, 214, 254, 225, 138, 4, 83, 236,
        159, 254, 225, 138, 4, 63, 87, 46, 254, 225, 138, 4, 63, 87, 56, 128, 225, 138, 4, 63, 87,
        56, 123, 225, 138, 4, 255, 255, 255, 255, 0, 0, 4, 255, 255, 255, 255, 0, 0, 4, 255, 255,
        255, 255, 0, 0, 4, 255, 255, 255, 255, 0, 0, 4, 255, 255, 255, 255, 0, 0, 4, 255, 255, 255,
        255, 0, 0, 4, 255, 255, 255, 255, 0, 0, 4, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 16, 151, 56, 146, 0, 0, 72, 0, 0, 0, 0, 0, 16, 151, 56, 146,
            };
            var p3 = new byte[]
            {
                132, 3, 0, 0, 0, 0, 72, 0, 0, 0, 0, 0, 16, 151, 56, 161
            };

            var p4 = new byte[] 
            {
        140, 4, 0, 0, 112, 44, 192, 2, 0, 0, 1, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 254, 236,
        189, 203, 114, 234, 202, 214, 239, 249, 157, 94, 85, 61, 198, 119, 186, 181, 79, 72, 2,
        188, 77, 245, 140, 145, 48, 24, 137, 137, 208, 5, 169, 162, 98, 7, 32, 246, 4, 36, 97, 77,
        155, 201, 173, 162, 158, 231, 188, 81, 53, 171, 89, 47, 80, 189, 211, 170, 148, 148, 194,
        164, 156, 96, 16, 248, 178, 214, 252, 55, 126, 177, 194, 107, 122, 88, 82, 94, 198, 200,
        113, 201, 204, 255, 231, 127, 252, 215, 255, 242, 31, 255, 241, 95, 254, 223, 255, 251,
        127, 252, 215, 255, 254, 63, 255, 199, 127, 252, 159, 255, 57, 154, 12, 166, 243, 255, 252,
        223, 254, 247, 255, 28, 111, 90, 147, 97, 99, 52, 237, 76, 91, 138, 185, 85, 203, 218, 125,
        243, 165, 57, 255, 41, 122, 189, 230, 77, 211, 23, 155, 118, 223, 83, 186, 86, 208, 236,
        73, 213, 65, 175, 31, 253, 82, 101, 173, 102, 90, 65, 163, 183, 213, 21, 221, 84, 234, 61,
        83, 119, 93, 65, 81, 172, 70, 107, 233, 25, 138, 210, 179, 149, 23, 79, 18, 205, 142, 41,
        8, 163, 134, 179, 177, 77, 87, 182, 131, 86, 121, 80, 106, 213, 135, 125, 179, 162, 139,
        122, 203, 241, 131, 138, 109, 4, 93, 51, 168, 173, 135, 74, 75, 29, 214, 29, 97, 104, 4,
        55, 222, 182, 86, 26, 205, 189, 242, 192, 127, 217, 90, 219, 174, 48, 40, 233, 115, 171,
        49, 105, 141, 109, 189, 63, 182, 91, 63, 70, 141, 167, 237, 120, 166, 148, 213, 173, 92,
        30, 110, 149, 78, 91, 12, 66, 93, 241, 5, 93, 208, 45, 45, 156, 56, 143, 130, 18, 90, 194,
        147, 104, 55, 116, 209, 8, 163, 103, 211, 94, 151, 134, 91, 189, 50, 174, 235, 191, 135,
        138, 213, 237, 218, 21, 211, 182, 252, 233, 191, 187, 79, 255, 141, 124, 247, 204, 237,
        183, 132, 129, 237, 70, 142, 164, 8, 174, 169, 136, 94, 99, 178, 28, 133, 129, 48, 38, 223,
        238, 61, 180, 68, 183, 183, 154, 186, 253, 201, 170, 57, 123, 90, 107, 179, 110, 169, 83,
        119, 214, 154, 33, 175, 219, 247, 173, 200, 109, 88, 191, 189, 6, 249, 93, 171, 38, 58,
        225, 58, 114, 132, 69, 48, 62, 179, 205, 58, 178, 165, 14, 164, 160, 60, 54, 215, 51, 79,
        90, 15, 70, 243, 192, 50, 109, 77, 84, 45, 93, 50, 229, 234, 162, 103, 180, 238, 181, 146,
        235, 116, 234, 218, 111, 183, 81, 233, 155, 129, 53, 177, 27, 66, 73, 123, 240, 26, 110,
        40, 139, 238, 180, 250, 226, 137, 74, 199, 106, 184, 27, 67, 113, 155, 142, 209, 234, 14,
        109, 235, 247, 72, 246, 90, 154, 31, 61, 245, 76, 209, 234, 133, 74, 223, 158, 183, 126,
        13, 77, 241, 87, 199, 168, 117, 134, 194, 162, 163, 7, 90, 167, 59, 183, 218, 110, 67, 40,
        143, 130, 32, 178, 31, 180, 208, 233, 63, 109, 123, 91, 85, 26, 223, 223, 174, 45, 163, 41,
        245, 30, 106, 15, 170, 18, 149, 123, 155, 170, 173, 90, 206, 148, 124, 243, 111, 39, 116,
        166, 157, 153, 44, 105, 245, 145, 212, 169, 255, 148, 52, 67, 169, 222, 255, 252, 111, 237,
        167, 127, 74, 191, 234, 254, 100, 169, 140, 42, 171, 201, 92, 173, 142, 252, 127, 77, 86,
        247, 77, 251, 95, 254, 175, 127, 215, 127, 223, 246, 42, 255, 26, 253, 235, 165, 108, 133,
        255, 246, 151, 203, 202, 211, 250, 151, 175, 12, 203, 193, 124, 221, 155, 255, 67, 126,
        190, 109, 253, 99, 242, 243, 95, 211, 167, 219, 187, 237, 157, 107, 254, 43, 24, 107, 81,
        79, 22, 171, 225, 38, 252, 183, 240, 24, 4, 255, 114, 31, 21, 213, 235, 11, 195, 81, 239,
        159, 253, 161, 97, 169, 222, 77, 215, 41, 173, 164, 231, 254, 68, 157, 151, 199, 131, 31,
        15, 51, 229, 63, 255, 215, 120, 20, 151, 181, 190, 25, 143, 98, 173, 167, 4, 37, 210, 202,
        110, 79, 94, 44, 109, 63, 184, 25, 204, 106, 245, 174, 165, 184, 186, 255, 34, 232, 166,
        85, 235, 10, 129, 108, 219, 94, 77, 55, 38, 138, 209, 88, 68, 227, 135, 64, 117, 74, 222,
        11, 105, 165, 138, 101, 41, 51, 50, 138, 77, 47, 168, 253, 24, 154, 254, 166, 75, 250, 197,
        174, 255, 220, 14, 67, 253, 183, 37, 182, 44, 199, 154, 44, 180, 173, 94, 210, 67, 119,
        162, 217, 238, 168, 45, 173, 75, 166, 89, 49, 221, 121, 75, 181, 252, 201, 68, 13, 106, 11,
        215, 174, 172, 116, 193, 151, 122, 225, 164, 101, 88, 214, 163, 99, 121, 131, 81, 24, 45,
        12, 251, 169, 162, 202, 149, 101, 79, 168, 52, 76, 161, 114, 239, 153, 11, 127, 104, 76,
        54, 182, 29, 56, 35, 201, 157, 140, 103, 90, 91, 221, 186, 229, 126, 190, 7, 196, 213, 210,
        154, 41, 118, 115, 186, 154, 58, 246, 122, 78, 70, 227, 84, 183, 212, 109, 167, 222, 37,
        35, 57, 30, 200, 156, 142, 145, 201, 36, 38, 19, 59, 212, 151, 67, 179, 178, 28, 134, 90,
        64, 196, 74, 234, 204, 41, 107, 51, 191, 172, 197, 19, 125, 214, 20, 200, 127, 5, 242, 251,
        66, 60, 160, 71, 37, 53, 105, 190, 161, 20, 17, 185, 81, 220, 225, 193, 248, 225, 46, 251,
        187, 228, 247, 101, 81, 141, 255, 46, 249, 91, 163, 185, 30, 185, 97, 48, 115, 250, 122,
        208, 237, 91, 194, 160, 81, 221, 12, 250, 122, 165, 57, 139, 132, 209, 220, 10, 226, 191,
        231, 244, 187, 251, 239, 84, 74, 101, 3, 159, 52, 143, 16, 255, 174, 249, 96, 77, 135, 141,
        96, 214, 147, 172, 74, 252, 73, 134, 57, 113, 61, 193, 170, 217, 230, 164, 61, 20, 163,
        214, 56, 124, 90, 117, 5, 69, 39, 10, 164, 173, 201, 150, 210, 53, 181, 150, 174, 4, 243,
        174, 105, 173, 134, 134, 86, 215, 238, 23, 45, 53, 208, 156, 81, 99, 97, 116, 172, 213,
        114, 40, 142, 54, 174, 229, 149, 188, 185, 190, 238, 9, 81, 89, 51, 69, 215, 182, 38, 134,
        107, 212, 108, 93, 146, 203, 35, 210, 197, 158, 111, 13, 6, 194, 122, 57, 18, 91, 226, 64,
        138, 106, 166, 160, 77, 6, 210, 66, 232, 17, 165, 52, 242, 29, 113, 220, 183, 212, 145,
        229, 170, 158, 82, 171, 140, 251, 110, 191, 107, 47, 38, 93, 59, 90, 143, 103, 147, 173,
        166, 232, 237, 158, 229, 106, 221, 112, 210, 115, 231, 234, 115, 135, 76, 126, 199, 22,
        213, 113, 99, 210, 28, 62, 68, 79, 182, 226, 174, 201, 179, 230, 214, 67, 107, 101, 217,
        238, 141, 19, 40, 125, 67, 80, 227, 46, 50, 255, 61, 31, 119, 199, 195, 137, 182, 49, 141,
        167, 40, 108, 255, 238, 169, 189, 127, 140, 171, 255, 124, 232, 206, 102, 155, 95, 102,
        235, 95, 255, 186, 123, 12, 122, 225, 116, 34, 213, 131, 141, 222, 105, 253, 90, 180, 199,
        255, 248, 167, 80, 119, 126, 215, 36, 177, 94, 173, 53, 167, 139, 142, 242, 115, 48, 155,
        141, 204, 91, 219, 239, 205, 31, 154, 119, 198, 248, 246, 121, 118, 59, 239, 135, 146, 166,
        248, 255, 26, 119, 234, 99, 233, 229, 241, 103, 125, 182, 157, 107, 178, 225, 63, 85, 158,
        22, 97, 224, 222, 254, 187, 62, 115, 111, 170, 139, 162, 19, 164, 95, 91, 168, 130, 42, 60,
        10, 254, 198, 20, 39, 171, 129, 168, 86, 250, 247, 213, 165, 181, 109, 5, 86, 201, 147,
        188, 7, 165, 221, 35, 166, 194, 176, 2, 215, 86, 180, 64, 85, 220, 7, 199, 152, 108, 137,
        42, 147, 116, 43, 122, 54, 26, 213, 173, 57, 183, 158, 7, 190, 98, 116, 37, 229, 153, 168,
        243, 182, 37, 147, 47, 156, 89, 21, 207, 92, 175, 45, 127, 45, 152, 15, 65, 197, 11, 188,
        218, 64, 82, 106, 131, 185, 114, 211, 121, 80, 5, 75, 94, 180, 44, 95, 188, 31, 40, 173,
        112, 180, 121, 41, 147, 209, 50, 25, 154, 235, 242, 64, 158, 188, 140, 137, 154, 183, 230,
        74, 160, 54, 60, 115, 52, 175, 89, 238, 60, 154, 154, 150, 247, 163, 187,
            };
            var p5 = new byte[] 
            {
        140, 5, 0, 0, 96, 9, 64, 1, 0, 0, 0, 0, 0, 0, 19, 4, 83, 237, 234, 82, 74, 188, 6, 23, 0,
        225, 138, 0, 0, 0, 0, 254, 128, 0, 0, 0, 0, 0, 0, 196, 178, 112, 86, 5, 59, 97, 219, 15, 0,
        0, 0, 6, 23, 0, 225, 138, 0, 0, 0, 0, 254, 128, 0, 0, 0, 0, 0, 0, 188, 210, 59, 150, 246,
        167, 182, 213, 33, 0, 0, 0, 6, 23, 0, 225, 138, 0, 0, 0, 0, 254, 128, 0, 0, 0, 0, 0, 0,
        132, 194, 47, 23, 175, 46, 78, 138, 23, 0, 0, 0, 6, 23, 0, 225, 138, 0, 0, 0, 0, 254, 128,
        0, 0, 0, 0, 0, 0, 136, 219, 85, 240, 191, 125, 172, 233, 10, 0, 0, 0, 6, 23, 0, 225, 138,
        0, 0, 0, 0, 254, 128, 0, 0, 0, 0, 0, 0, 80, 211, 212, 44, 191, 227, 124, 40, 13, 0, 0, 0,
        6, 23, 0, 225, 138, 0, 0, 0, 0, 254, 128, 0, 0, 0, 0, 0, 0, 77, 13, 19, 149, 102, 140, 134,
        77, 16, 0, 0, 0, 4, 83, 237, 239, 254, 225, 138, 4, 63, 87, 214, 254, 225, 138, 4, 83, 236,
        159, 254, 225, 138, 4, 63, 87, 46, 254, 225, 138, 4, 63, 87, 56, 128, 225, 138, 4, 63, 87,
        56, 123, 225, 138, 4, 255, 255, 255, 255, 0, 0, 4, 255, 255, 255, 255, 0, 0, 4, 255, 255,
        255, 255, 0, 0, 4, 255, 255, 255, 255, 0, 0, 4, 255, 255, 255, 255, 0, 0, 4, 255, 255, 255,
        255, 0, 0, 4, 255, 255, 255, 255, 0, 0, 4, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 16, 151, 56, 146,
            };
            var p6 = new byte[]
            {
        140, 6, 0, 0, 112, 44, 192, 2, 0, 0, 1, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 254, 236,
        189, 203, 114, 234, 202, 214, 239, 249, 157, 94, 85, 61, 198, 119, 186, 181, 79, 72, 2,
        188, 77, 245, 140, 145, 48, 24, 137, 137, 208, 5, 169, 162, 98, 7, 32, 246, 4, 36, 97, 77,
        155, 201, 173, 162, 158, 231, 188, 81, 53, 171, 89, 47, 80, 189, 211, 170, 148, 148, 194,
        164, 156, 96, 16, 248, 178, 214, 252, 55, 126, 177, 194, 107, 122, 88, 82, 94, 198, 200,
        113, 201, 204, 255, 231, 127, 252, 215, 255, 242, 31, 255, 241, 95, 254, 223, 255, 251,
        127, 252, 215, 255, 254, 63, 255, 199, 127, 252, 159, 255, 57, 154, 12, 166, 243, 255, 252,
        223, 254, 247, 255, 28, 111, 90, 147, 97, 99, 52, 237, 76, 91, 138, 185, 85, 203, 218, 125,
        243, 165, 57, 255, 41, 122, 189, 230, 77, 211, 23, 155, 118, 223, 83, 186, 86, 208, 236,
        73, 213, 65, 175, 31, 253, 82, 101, 173, 102, 90, 65, 163, 183, 213, 21, 221, 84, 234, 61,
        83, 119, 93, 65, 81, 172, 70, 107, 233, 25, 138, 210, 179, 149, 23, 79, 18, 205, 142, 41,
        8, 163, 134, 179, 177, 77, 87, 182, 131, 86, 121, 80, 106, 213, 135, 125, 179, 162, 139,
        122, 203, 241, 131, 138, 109, 4, 93, 51, 168, 173, 135, 74, 75, 29, 214, 29, 97, 104, 4,
        55, 222, 182, 86, 26, 205, 189, 242, 192, 127, 217, 90, 219, 174, 48, 40, 233, 115, 171,
        49, 105, 141, 109, 189, 63, 182, 91, 63, 70, 141, 167, 237, 120, 166, 148, 213, 173, 92,
        30, 110, 149, 78, 91, 12, 66, 93, 241, 5, 93, 208, 45, 45, 156, 56, 143, 130, 18, 90, 194,
        147, 104, 55, 116, 209, 8, 163, 103, 211, 94, 151, 134, 91, 189, 50, 174, 235, 191, 135,
        138, 213, 237, 218, 21, 211, 182, 252, 233, 191, 187, 79, 255, 141, 124, 247, 204, 237,
        183, 132, 129, 237, 70, 142, 164, 8, 174, 169, 136, 94, 99, 178, 28, 133, 129, 48, 38, 223,
        238, 61, 180, 68, 183, 183, 154, 186, 253, 201, 170, 57, 123, 90, 107, 179, 110, 169, 83,
        119, 214, 154, 33, 175, 219, 247, 173, 200, 109, 88, 191, 189, 6, 249, 93, 171, 38, 58,
        225, 58, 114, 132, 69, 48, 62, 179, 205, 58, 178, 165, 14, 164, 160, 60, 54, 215, 51, 79,
        90, 15, 70, 243, 192, 50, 109, 77, 84, 45, 93, 50, 229, 234, 162, 103, 180, 238, 181, 146,
        235, 116, 234, 218, 111, 183, 81, 233, 155, 129, 53, 177, 27, 66, 73, 123, 240, 26, 110,
        40, 139, 238, 180, 250, 226, 137, 74, 199, 106, 184, 27, 67, 113, 155, 142, 209, 234, 14,
        109, 235, 247, 72, 246, 90, 154, 31, 61, 245, 76, 209, 234, 133, 74, 223, 158, 183, 126,
        13, 77, 241, 87, 199, 168, 117, 134, 194, 162, 163, 7, 90, 167, 59, 183, 218, 110, 67, 40,
        143, 130, 32, 178, 31, 180, 208, 233, 63, 109, 123, 91, 85, 26, 223, 223, 174, 45, 163, 41,
        245, 30, 106, 15, 170, 18, 149, 123, 155, 170, 173, 90, 206, 148, 124, 243, 111, 39, 116,
        166, 157, 153, 44, 105, 245, 145, 212, 169, 255, 148, 52, 67, 169, 222, 255, 252, 111, 237,
        167, 127, 74, 191, 234, 254, 100, 169, 140, 42, 171, 201, 92, 173, 142, 252, 127, 77, 86,
        247, 77, 251, 95, 254, 175, 127, 215, 127, 223, 246, 42, 255, 26, 253, 235, 165, 108, 133,
        255, 246, 151, 203, 202, 211, 250, 151, 175, 12, 203, 193, 124, 221, 155, 255, 67, 126,
        190, 109, 253, 99, 242, 243, 95, 211, 167, 219, 187, 237, 157, 107, 254, 43, 24, 107, 81,
        79, 22, 171, 225, 38, 252, 183, 240, 24, 4, 255, 114, 31, 21, 213, 235, 11, 195, 81, 239,
        159, 253, 161, 97, 169, 222, 77, 215, 41, 173, 164, 231, 254, 68, 157, 151, 199, 131, 31,
        15, 51, 229, 63, 255, 215, 120, 20, 151, 181, 190, 25, 143, 98, 173, 167, 4, 37, 210, 202,
        110, 79, 94, 44, 109, 63, 184, 25, 204, 106, 245, 174, 165, 184, 186, 255, 34, 232, 166,
        85, 235, 10, 129, 108, 219, 94, 77, 55, 38, 138, 209, 88, 68, 227, 135, 64, 117, 74, 222,
        11, 105, 165, 138, 101, 41, 51, 50, 138, 77, 47, 168, 253, 24, 154, 254, 166, 75, 250, 197,
        174, 255, 220, 14, 67, 253, 183, 37, 182, 44, 199, 154, 44, 180, 173, 94, 210, 67, 119,
        162, 217, 238, 168, 45, 173, 75, 166, 89, 49, 221, 121, 75, 181, 252, 201, 68, 13, 106, 11,
        215, 174, 172, 116, 193, 151, 122, 225, 164, 101, 88, 214, 163, 99, 121, 131, 81, 24, 45,
        12, 251, 169, 162, 202, 149, 101, 79, 168, 52, 76, 161, 114, 239, 153, 11, 127, 104, 76,
        54, 182, 29, 56, 35, 201, 157, 140, 103, 90, 91, 221, 186, 229, 126, 190, 7, 196, 213, 210,
        154, 41, 118, 115, 186, 154, 58, 246, 122, 78, 70, 227, 84, 183, 212, 109, 167, 222, 37,
        35, 57, 30, 200, 156, 142, 145, 201, 36, 38, 19, 59, 212, 151, 67, 179, 178, 28, 134, 90,
        64, 196, 74, 234, 204, 41, 107, 51, 191, 172, 197, 19, 125, 214, 20, 200, 127, 5, 242, 251,
        66, 60, 160, 71, 37, 53, 105, 190, 161, 20, 17, 185, 81, 220, 225, 193, 248, 225, 46, 251,
        187, 228, 247, 101, 81, 141, 255, 46, 249, 91, 163, 185, 30, 185, 97, 48, 115, 250, 122,
        208, 237, 91, 194, 160, 81, 221, 12, 250, 122, 165, 57, 139, 132, 209, 220, 10, 226, 191,
        231, 244, 187, 251, 239, 84, 74, 101, 3, 159, 52, 143, 16, 255, 174, 249, 96, 77, 135, 141,
        96, 214, 147, 172, 74, 252, 73, 134, 57, 113, 61, 193, 170, 217, 230, 164, 61, 20, 163,
        214, 56, 124, 90, 117, 5, 69, 39, 10, 164, 173, 201, 150, 210, 53, 181, 150, 174, 4, 243,
        174, 105, 173, 134, 134, 86, 215, 238, 23, 45, 53, 208, 156, 81, 99, 97, 116, 172, 213,
        114, 40, 142, 54, 174, 229, 149, 188, 185, 190, 238, 9, 81, 89, 51, 69, 215, 182, 38, 134,
        107, 212, 108, 93, 146, 203, 35, 210, 197, 158, 111, 13, 6, 194, 122, 57, 18, 91, 226, 64,
        138, 106, 166, 160, 77, 6, 210, 66, 232, 17, 165, 52, 242, 29, 113, 220, 183, 212, 145,
        229, 170, 158, 82, 171, 140, 251, 110, 191, 107, 47, 38, 93, 59, 90, 143, 103, 147, 173,
        166, 232, 237, 158, 229, 106, 221, 112, 210, 115, 231, 234, 115, 135, 76, 126, 199, 22,
        213, 113, 99, 210, 28, 62, 68, 79, 182, 226, 174, 201, 179, 230, 214, 67, 107, 101, 217,
        238, 141, 19, 40, 125, 67, 80, 227, 46, 50, 255, 61, 31, 119, 199, 195, 137, 182, 49, 141,
        167, 40, 108, 255, 238, 169, 189, 127, 140, 171, 255, 124, 232, 206, 102, 155, 95, 102,
        235, 95, 255, 186, 123, 12, 122, 225, 116, 34, 213, 131, 141, 222, 105, 253, 90, 180, 199,
        255, 248, 167, 80, 119, 126, 215, 36, 177, 94, 173, 53, 167, 139, 142, 242, 115, 48, 155,
        141, 204, 91, 219, 239, 205, 31, 154, 119, 198, 248, 246, 121, 118, 59, 239, 135, 146, 166,
        248, 255, 26, 119, 234, 99, 233, 229, 241, 103, 125, 182, 157, 107, 178, 225, 63, 85, 158,
        22, 97, 224, 222, 254, 187, 62, 115, 111, 170, 139, 162, 19, 164, 95, 91, 168, 130, 42, 60,
        10, 254, 198, 20, 39, 171, 129, 168, 86, 250, 247, 213, 165, 181, 109, 5, 86, 201, 147,
        188, 7, 165, 221, 35, 166, 194, 176, 2, 215, 86, 180, 64, 85, 220, 7, 199, 152, 108, 137,
        42, 147, 116, 43, 122, 54, 26, 213, 173, 57, 183, 158, 7, 190, 98, 116, 37, 229, 153, 168,
        243, 182, 37, 147, 47, 156, 89, 21, 207, 92, 175, 45, 127, 45, 152, 15, 65, 197, 11, 188,
        218, 64, 82, 106, 131, 185, 114, 211, 121, 80, 5, 75, 94, 180, 44, 95, 188, 31, 40, 173,
        112, 180, 121, 41, 147, 209, 50, 25, 154, 235, 242, 64, 158, 188, 140, 137, 154, 183, 230,
        74, 160, 54, 60, 115, 52, 175, 89, 238, 60, 154, 154, 150, 247, 163, 187,
            };

            var ps = new List<byte[]>
            {
                p0, p1, p2, p3, p4, p5, p6
            };

            int n = 0;
            var rq = new RecvQ();
            foreach (var i in ps)
            {
                var v = new FrameVec(i);
                foreach (var item in v.frames)
                {
                    rq.Insert(item);
                    if (rq.Flush(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0)).Count != 0)
                    {
                        n++;
                    }
                }
            }
            if (n == 5)
            {
                return true;
            }
            return false;
        }

        static bool TestSerializeDeserialize()
        {
            var p = new byte[]
            {
        132, 0, 0, 0, 64, 0, 144, 0, 0, 0, 9, 146, 33, 7, 47, 57, 18, 128, 111, 0, 0, 0, 0, 20,
        200, 47, 41, 0,
            };
            var a = FrameSetPacket.Deserialize(p);
            var s = a.Serialize();
            if (s.SequenceEqual(p))
            {
                return true;
            }
            return false;
        }
    }
}
