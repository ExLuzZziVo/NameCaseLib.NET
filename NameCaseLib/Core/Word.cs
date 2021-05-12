#region

using NameCaseLib.NCL;

#endregion

namespace NameCaseLib.Core
{
    /// <summary>
    ///     Word - класс, который служит для хранения всей информации о каждом слове
    /// </summary>
    public class Word
    {
        /// <summary>
        ///     Слово в нижнем регистре, которое хранится в об’єкте класса
        /// </summary>
        private readonly string word;

        /// <summary>
        ///     Окончательное решение, к какому полу относится слово
        /// </summary>
        private Gender genderSolved = Gender.Null;

        /// <summary>
        ///     Содержит true, если все слово было в верхнем регистре и false, если не было
        /// </summary>
        private bool isUpperCase;

        /// <summary>
        ///     Маска больших букв в слове.
        ///     Содержит информацию о том, какие буквы в слове были большими, а какие мальникими:
        ///     - x - маленькая буква
        ///     - X - больная буква
        /// </summary>
        private LettersMask[] letterMask;

        /// <summary>
        ///     Вероятность того, что текущей слово относится к или женскому полу
        /// </summary>
        private GenderProbability manOrWoman;

        /// <summary>
        ///     Массив содержит все падежи слова, полученые после склонения текущего слова
        /// </summary>
        private string[] nameCases;

        /// <summary>
        ///     Тип текущей записи (Фамилия/Имя/Отчество)
        /// </summary>
        private NamePart namePart = NamePart.Null;

        /// <summary>
        ///     Номер правила, по которому было произведено склонение текущего слова
        /// </summary>
        private int rule;

        /// <summary>
        ///     Создание нового обьекта со словом
        /// </summary>
        /// <param name="word">Слово</param>
        public Word(string word)
        {
            GenerateMask(word);
            this.word = word.ToLower();
        }

        /// <summary>
        ///     Считывает или устанавливает все падежи
        /// </summary>
        public string[] NameCases
        {
            set
            {
                nameCases = value;
                ReturnMask();
            }
            get => nameCases;
        }

        /// <summary>
        ///     Расчитывает и возвращает пол текущего слова. Или устанавливает нужный пол.
        /// </summary>
        public Gender Gender
        {
            get
            {
                if (genderSolved == Gender.Null)
                {
                    if (manOrWoman.Man > manOrWoman.Woman)
                    {
                        genderSolved = Gender.Man;
                    }
                    else
                    {
                        genderSolved = Gender.Woman;
                    }
                }

                return genderSolved;
            }
            set => genderSolved = value;
        }

        /// <summary>
        ///     Устанавливает вероятности того, что текущий челове мужчина или женщина
        /// </summary>
        public GenderProbability GenderProbability
        {
            get => manOrWoman;
            set => manOrWoman = value;
        }

        /// <summary>
        ///     Возвращает или устанавливает идентификатор части ФИО
        /// </summary>
        public NamePart NamePart
        {
            get => namePart;
            set => namePart = value;
        }

        /// <summary>
        ///     Текущее слово
        /// </summary>
        public string Name => word;

        /// <summary>
        ///     Устанавливает или считывает правило склонения текущего слова
        /// </summary>
        public int Rule
        {
            get => rule;
            set => rule = value;
        }

        /// <summary>
        ///     Генерирует маску, которая содержит информацию о том, какие буквы в слове были большими, а какие маленькими:
        ///     - x - маленькая буква
        ///     - Х - большая буква
        /// </summary>
        /// <param name="word">Слово для которого нужна маска</param>
        private void GenerateMask(string word)
        {
            isUpperCase = true;
            var length = word.Length;
            letterMask = new LettersMask[length];

            for (var i = 0; i < length; i++)
            {
                var letter = word.Substring(i, 1);

                if (Str.isLowerCase(letter))
                {
                    isUpperCase = false;
                    letterMask[i] = LettersMask.x;
                }
                else
                {
                    letterMask[i] = LettersMask.X;
                }
            }
        }

        /// <summary>
        ///     Возвращает все падежи слова в начальную маску
        /// </summary>
        private void ReturnMask()
        {
            var wordCount = nameCases.Length;

            if (isUpperCase)
            {
                for (var i = 0; i < wordCount; i++)
                {
                    nameCases[i] = nameCases[i].ToUpper();
                }
            }
            else
            {
                for (var i = 0; i < wordCount; i++)
                {
                    var lettersCount = nameCases[i].Length;
                    var maskLength = letterMask.Length;
                    var newStr = "";

                    for (var letter = 0; letter < lettersCount; letter++)
                    {
                        if (letter < maskLength && letterMask[letter] == LettersMask.X)
                        {
                            newStr += nameCases[i].Substring(letter, 1).ToUpper();
                        }
                        else
                        {
                            newStr += nameCases[i].Substring(letter, 1);
                        }
                    }

                    nameCases[i] = newStr;
                }
            }
        }

        /// <summary>
        ///     Возвращает строку с нужным падежом текущего слова
        /// </summary>
        /// <param name="padeg">нужный падеж</param>
        /// <returns>строка с нужным падежом текущего слова</returns>
        public string GetNameCase(Padeg padeg)
        {
            return nameCases[(int) padeg];
        }

        /// <summary>
        ///     Если уже был расчитан пол для всех слов системы, тогда каждому слову предается окончательное
        ///     решение. Эта функция определяет было ли принято окончательное решение.
        /// </summary>
        /// <returns>true если определен и false если нет</returns>
        public bool isGenderSolved()
        {
            if (genderSolved == Gender.Null)
            {
                return false;
            }

            return true;
        }
    }
}