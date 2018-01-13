using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProgrammingTest
{
    /*
    Instructions: Welcome to the WayForward Unity Programmer Test.  This test
    consists of 3 parts.  
    
    First, Answer the questions in the AnswerMe region

    Second, follow the instructions in the ImplementTheseFunctions
    region below. 
    
    Third, Implement a simple Match 3 using Unity UI (Canvas based UI).
    The Match3 should function in 16:9 portrait mode (so 9:16)
    Match 3 features requires:
        * 5 different tile types - Starting point prefabs provided in Prefab folder, modify as necessary
        * A 9x9 grid that gets randomly populated with the tiles
        * The Ability for the player to touch a tile and swap it with an adjacent tile
        * The Functionality to detect matches
            * 3 or more tiles of the same type adjacent to each other (horizontally or vertically)
            * Matches that overlap (such as a T shape) should count as a single match (of 5 tiles)
        * Giving points based on matches
        * Removing matched tiles
        * Moving tiles down to fill empty spots
        * Randomly spawning tiles at the top row to fill empty spots there
    A very limited start has been provided in the Match3 scene, TODO comments have been left in
    Grid.cs to help you get started.
    */
    #region AnswerMe

    /*
    Answer the following questions in the space below each.

    1. What is an if statement

    1a. How is a while statement different from an if statement?

    1b. How is a while statement different from a for statement?

    2. What is the difference between an Array and a Linked List?

    2a. Give an example of when you would use an Array instead of a Linked List.

    2b. Give an example of when you would use a Linked List instead of an Array.

    */

    #endregion

    #region ImplementTheseFunctions

    /// <summary>
    /// 1. Given an int, return an int that has the same bytes but in reverse order.
    /// This is sometimes referred to as swapping Endianness.
    /// </summary>
    /// <param name="value">Original Int</param>
    /// <returns>Int with reversed bytes</returns>
    public static int ReverseBytes(int value)
    {
        return 0;
    }

    /*
    Imagine you were making a black jack game.  Define the data structure you would use for the cards.
    */

    /// <summary>
    /// 2. Class representing a single card for a card game.
    /// </summary>
    public class Card
    {
    }

    /// <summary>
    /// 3. Function that shuffles a deck of cards by modifying the array in place 
    /// </summary>
    /// <param name="deckOf52">Array of 52 Cards to be shuffled</param>
    public static void ShuffleCards(Card[] deckOf52)
    {
    }

    /// <summary>
    /// 4. Given an array of chars, count the number of times a char representing a
    /// hexadecimal digit appears (chars '0'-'9' and 'a'-'f' or 'A'-'F'). Store the
    /// count for each hexadecimal digit in the array of 16 ints passed in.
    /// Counts should be stored such that index 0x0 is the count '0' shows up and 0xf
    /// is the count 'f' or 'F' shows up and so on.
    /// For example, if the array of Chars is { 'W', 'a', 'y', 'F', 'o', 'r', 'w', 'a', 'r', 'd' }
    /// The output should be {  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  2,  0,  0,  1,  0,  1 }
    /// Indices               0x0 0x1 0x2 0x3 0x4 0x5 0x6 0x7 0x8 0x9 0xA 0xB 0xC 0xD 0xE 0xF
    /// You will first be graded on ACCURACY and then graded on SPEED of your algorithm
    /// </summary>
    /// <param name="arrayOfChars">An array of characters</param>
    /// <param name="arrayOfCounts">An array for the count each hex character (0-f) shows up</param>
    public static void CountHexDigits(char[] arrayOfChars, int[] arrayOfCounts)
    {
        return;
    }

    /// <summary>
    /// Given 2 normalized 3d Vectors caculate, using the Dot and Cross products, the Angle-Axis 
    /// rotation that will transform the first vector into the second.
    /// </summary>
    /// <param name="first">The Original Vector</param>
    /// <param name="second">The Vector to transform the first into</param>
    /// <param name="rotationAxis">Axis of rotation</param>
    /// <param name="rotationAngle">Angle of rotation</param>
    public static void CalculateAngleAxisDifference(Vector3 first, Vector3 second, out Vector3 rotationAxis, out float rotationAngle)
    {
        rotationAngle = 0f;
        rotationAxis = Vector3.up;
    }

    /// <summary>
    /// A bug has been isolated to the below function:  
    /// Objects using this function will stop moving despite velocity not being 0.
    /// The bug is known to occur under the following conditions:
    /// position 600000.0f, velocity 1.0f, Fixed Timestep set to 0.02.
    /// Identify the issue and propose a solution as a comment.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="velocity"></param>
    /// <returns></returns>
    public static float Move(float position, float velocity)
    {
        return position + velocity * Time.fixedDeltaTime;
    }
    
    #endregion

}
