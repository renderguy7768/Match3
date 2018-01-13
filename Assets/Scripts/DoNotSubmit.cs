using UnityEngine;

public class DoNotSubmit : MonoBehaviour
{
    public int ReverseByteVal;

    #region ImplementTheseFunctions

    /// <summary>
    /// 1. Given an int, return an int that has the same bytes but in reverse order.
    /// This is sometimes referred to as swapping Endianness.
    /// </summary>
    /// <param name="value">Original Int</param>
    /// <returns>Int with reversed bytes</returns>
    public static int ReverseBytes(int value)
    {
        var unsignedValue = (uint)value;
        return (int)(
            (unsignedValue >> 24) & 0x000000ff |
            (unsignedValue << 08) & 0x00ff0000 |
            (unsignedValue >> 08) & 0x0000ff00 |
            (unsignedValue << 24) & 0xff000000
            );
    }

    /*
    Imagine you were making a black jack game.  Define the data structure you would use for the cards.
    */

    /// <summary>
    /// 2. Class representing a single card for a card game.
    /// </summary>
    public class Card
    {
        public enum Suit : byte
        {
            Spades,
            Hearts,
            Diamonds,
            Clubs
        }
        public enum Value : byte
        {
            Ace = 1,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Ten,
            Jack,
            Queen,
            King
        }

        public Suit CardSuit { get; private set; }
        public Value CardValue { get; private set; }

        public Card(Suit cardSuit, Value cardValue)
        {
            CardSuit = cardSuit;
            CardValue = cardValue;
        }

        public override string ToString()
        {
            return "Suit: " + CardSuit + " Value: " + CardValue;
        }
    }

    public class Deck
    {
        private readonly Card[] _cards;
        private const byte NumberOfSuits = 4;
        private const byte NumberOfCardsPerSuit = 13;
        private const byte NumberOfCards = 52;
        public Deck()
        {
            _cards = new Card[NumberOfCards];
            for (var i = 0; i < NumberOfSuits; i++)
            {
                for (var j = 1; j <= NumberOfCardsPerSuit; j++)
                {
                    _cards[j - 1 + i * NumberOfCardsPerSuit] = new Card((Card.Suit)i, (Card.Value)j);
                }
            }
        }

        public void Shuffle()
        {
            var random = new System.Random();
            for (var i = NumberOfCards - 1; i >= 0; i--)
            {
                var j = random.Next(0, i + 1);
                var temp = _cards[j];
                _cards[j] = _cards[i];
                _cards[i] = temp;
            }
        }

        public void PrintAllCards()
        {
            for (var i = 0; i < NumberOfCards; i++)
            {
                print(_cards[i].ToString());
            }
        }
    }

    /// <summary>
    /// 3. Function that shuffles a deck of cards by modifying the array in place 
    /// </summary>
    /// <param name="deckOf52">Array of 52 Cards to be shuffled</param>
    public static void ShuffleCards(Card[] deckOf52)
    {
        var random = new System.Random();
        for (var i = 51; i >= 0; i--)
        {
            var j = random.Next(0, i + 1);
            var temp = deckOf52[j];
            deckOf52[j] = deckOf52[i];
            deckOf52[i] = temp;
        }
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
        var length = arrayOfChars.Length;
        for (var i = 0; i < length; i++)
        {
            if (arrayOfChars[i] <= '9')
            {
                var digit = arrayOfChars[i] - '0';
                if (digit >= 0)
                {
                    arrayOfCounts[digit]++;
                }
            }
            else if (arrayOfChars[i] <= 'F')
            {
                var uppercase = arrayOfChars[i] - 'A';
                if (uppercase >= 0)
                {
                    arrayOfCounts[10 + uppercase]++;
                }
            }
            else if (arrayOfChars[i] <= 'f')
            {
                var lowercase = arrayOfChars[i] - 'a';
                if (lowercase >= 0)
                {
                    arrayOfCounts[10 + lowercase]++;
                }
            }
        }

        for (var i = 0; i < arrayOfCounts.Length; i++)
        {
            Debug.Log(i + " " + arrayOfCounts[i]);
        }
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

    private void Start()
    {
        //Debug.LogFormat("{0}", ReverseByteVal.ToString("X8"));
        //Debug.LogFormat("{0}", ReverseBytes(ReverseByteVal).ToString("X8"));
        //var a = new Card[52];
        //ShuffleCards(a);
        var chararr = new[]
        {
            'W', 'a', 'y', 'F', 'o', 'r', 'w', 'a', 'r', 'd',
            '1', '0', '2', '2', 'f', 'a', 'X', 'Z', 'c', 'C',
            '/','.','1','5','@','?',':',';','g','G','`','_'
        };
        var countarr = new int[16];
        CountHexDigits(chararr, countarr);
    }
}
