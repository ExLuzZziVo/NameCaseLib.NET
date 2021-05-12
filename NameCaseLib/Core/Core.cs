#region

using System.Reflection;
using NameCaseLib.NCL;

#endregion

namespace NameCaseLib.Core
{
    /// <summary>
    ///     Набор основных функций, который позволяют сделать интерфейс слонения русского и украниского языка
    ///     абсолютно одинаковым. Содержит все функции для внешнего взаимодействия с библиотекой.
    /// </summary>
    public abstract class Core
    {
        /// <summary>
        ///     Версия библиотеки
        /// </summary>
        private static readonly string version = "0.2/0.4.1";

        /// <summary>
        ///     Версия языкового файла
        /// </summary>
        protected static string languageBuild;

        /// <summary>
        ///     Количество падежей в языке
        /// </summary>
        protected int caseCount;

        /// <summary>
        ///     Если все текущие слова было просклонены и в каждом слове уже есть результат склонения,
        ///     тогда true. Если было добавлено новое слово флаг збрасывается на false
        /// </summary>
        private bool finished;

        /// <summary>
        ///     Массив содержит результат склонения слова - слово во всех падежах
        /// </summary>
        protected string[] lastResult;

        /// <summary>
        ///     Номер последнего использованого правила, устанавливается методом Rule()
        /// </summary>
        private int lastRule;

        /// <summary>
        ///     Готовность системы:
        ///     - Все слова идентифицированы (известо к какой части ФИО относится слово)
        ///     - У всех слов определен пол
        ///     Если все сделано стоит флаг true, при добавлении нового слова флаг сбрасывается на false
        /// </summary>
        private bool ready;

        /// <summary>
        ///     Массив содержит елементы типа Word. Это все слова которые нужно обработать и просклонять
        /// </summary>
        private WordArray words;

        /// <summary>
        ///     Метод Last() вырезает подстроки разной длины. Посколько одинаковых вызовов бывает несколько,
        ///     то все результаты выполнения кешируются в этом массиве.
        /// </summary>
        private LastCache workindLastCache;

        /// <summary>
        ///     Переменная, в которую заносится слово с которым сейчас идет работа
        /// </summary>
        protected string workingWord;

        /// <summary>
        ///     Возвращает текущую версию библиотке
        /// </summary>
        public static string Version => version;

        /// <summary>
        ///     Возвращает текущую версию языкового файла
        /// </summary>
        public static string LanguageVersion => version;

        /// <summary>
        ///     Метод очищает результаты последнего склонения слова. Нужен при склонении нескольких слов.
        /// </summary>
        private void Reset()
        {
            lastRule = 0;
            lastResult = new string[caseCount];
        }

        /// <summary>
        ///     Устанавливает флаги о том, что система не готово и слова еще не были просклонены
        /// </summary>
        private void NotReady()
        {
            ready = false;
            finished = false;
        }

        /// <summary>
        ///     Сбрасывает все информацию на начальную. Очищает все слова добавленые в систему.
        ///     После выполнения система готова работать с начала.
        /// </summary>
        public Core FullReset()
        {
            words = new WordArray();
            Reset();
            NotReady();

            return this;
        }

        /// <summary>
        ///     Устанавливает последнее правило
        /// </summary>
        /// <param name="ruleID">Правило</param>
        protected void Rule(int ruleID)
        {
            lastRule = ruleID;
        }

        /// <summary>
        ///     Считывает и устанавливает последние правило
        /// </summary>
        protected int GetRule()
        {
            return lastRule;
        }

        /// <summary>
        ///     Устанавливает слово текущим для работы системы. Очищает кеш слова.
        /// </summary>
        /// <param name="word">слово, которое нужно установить</param>
        protected void SetWorkingWord(string word)
        {
            Reset();
            workingWord = word;
            workindLastCache = new LastCache();
        }

        /// <summary>
        ///     Если не нужно склонять слово, делает результат таким же как и именительный падеж
        /// </summary>
        protected void MakeResultTheSame()
        {
            lastResult = new string[caseCount];

            for (var i = 0; i < caseCount; i++)
            {
                lastResult[i] = workingWord;
            }
        }

        /// <summary>
        ///     Вырезает определенное количество последних букв текущего слова
        /// </summary>
        /// <param name="length">Количество букв</param>
        /// <returns>Подстроку содержущую определенное количество букв</returns>
        protected string Last(int length)
        {
            var result = workindLastCache.Get(length, length);

            if (result == "")
            {
                var startIndex = workingWord.Length - length;

                if (startIndex >= 0)
                {
                    result = workingWord.Substring(workingWord.Length - length, length);
                }
                else
                {
                    result = workingWord;
                }

                workindLastCache.Push(result, length, length);
            }

            return result;
        }

        /// <summary>
        ///     Получает указаное количество букв с конца слова
        /// </summary>
        /// <param name="word">Слово</param>
        /// <param name="length">Количество букв</param>
        /// <returns>Полученая строка</returns>
        protected string Last(string word, int length)
        {
            string result;

            var startIndex = word.Length - length;

            if (startIndex >= 0)
            {
                result = word.Substring(word.Length - length, length);
            }
            else
            {
                result = word;
            }

            return result;
        }

        /// <summary>
        ///     Вырезает stopAfter букв начиная с length с конца
        /// </summary>
        /// <param name="length">На сколько букв нужно оступить от конца</param>
        /// <param name="stopAfter">Сколько букв нужно вырезать</param>
        /// <returns>Искомоя строка</returns>
        protected string Last(int length, int stopAfter)
        {
            var result = workindLastCache.Get(length, stopAfter);

            if (result == "")
            {
                var startIndex = workingWord.Length - length;

                if (startIndex >= 0)
                {
                    result = workingWord.Substring(workingWord.Length - length, stopAfter);
                }
                else
                {
                    result = workingWord;
                }

                workindLastCache.Push(result, length, stopAfter);
            }

            return result;
        }

        /// <summary>
        ///     Извлекает последние буквы из указаного слова
        /// </summary>
        /// <param name="word">Слово</param>
        /// <param name="length">Сколько букв с конца</param>
        /// <param name="stopAfter">Сколько нужно взять</param>
        /// <returns>Полученая подстрока</returns>
        protected string Last(string word, int length, int stopAfter)
        {
            string result;
            var startIndex = word.Length - length;

            if (startIndex >= 0)
            {
                result = word.Substring(word.Length - length, stopAfter);
            }
            else
            {
                result = word;
            }

            return result;
        }

        /// <summary>
        ///     Над текущим словом выполняются правила в указаном порядке.
        /// </summary>
        /// <param name="gender">Пол текущего слова</param>
        /// <param name="rulesArray">Порядок правил</param>
        /// <returns>Если правило было использовао true если нет тогда false</returns>
        protected bool RulesChain(Gender gender, int[] rulesArray)
        {
            if (gender != Gender.Null)
            {
                var rulesLength = rulesArray.Length;
                var rulesName = gender == Gender.Man ? "Man" : "Woman";
                var classType = GetType();

                for (var i = 0; i < rulesLength; i++)
                {
                    var methodName = string.Format("{0}Rule{1}", rulesName, rulesArray[i]);

                    var res = (bool) classType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(this, null);

                    if (res)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Проверяет входит ли буква в список букв
        /// </summary>
        /// <param name="needle">буква</param>
        /// <param name="letters">список букв</param>
        /// <returns>true если входит в список и false если не входит</returns>
        protected bool In(string needle, string letters)
        {
            if (needle != "")
            {
                if (letters.IndexOf(needle) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Ищет окончание в списке окончаний
        /// </summary>
        /// <param name="needle">окончание</param>
        /// <param name="haystack">список окончаний</param>
        /// <returns>true если найдено и false если нет</returns>
        protected bool In(string needle, string[] haystack)
        {
            var length = haystack.Length;

            if (needle != "")
            {
                for (var i = 0; i < length; i++)
                {
                    if (haystack[i] == needle)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Проверяет равенство имени
        /// </summary>
        /// <param name="needle">имя</param>
        /// <param name="name">имя с которым сравнить</param>
        /// <returns>если имена совапали true</returns>
        protected bool InNames(string needle, string name)
        {
            if (needle == name)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Проверяет входит ли имя в список имен
        /// </summary>
        /// <param name="needle">имя</param>
        /// <param name="names">список имен</param>
        /// <returns>true если входит</returns>
        protected bool InNames(string needle, string[] names)
        {
            var length = names.Length;

            for (var i = 0; i < length; i++)
            {
                if (needle == names[i])
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Склоняем слово во все падежи используя окончания
        /// </summary>
        /// <param name="word">Слово</param>
        /// <param name="endings">окончания</param>
        /// <param name="replaceLast">сколько последних букв нужно убрать</param>
        protected void WordForms(string word, string[] endings, int replaceLast)
        {
            //Сохраняем именительный падеж
            lastResult = new string[caseCount];
            lastResult[0] = workingWord;

            if (word.Length >= replaceLast)
            {
                word = word.Substring(0, word.Length - replaceLast);
            }
            else
            {
                word = "";
            }

            //Приписуем окончания
            for (var i = 1; i < caseCount; i++)
            {
                lastResult[i] = word + endings[i - 1];
            }
        }

        /// <summary>
        ///     Создает список слов во всех падежах используя окончания для каждого падежа
        /// </summary>
        /// <param name="word">слово</param>
        /// <param name="endings">окончания</param>
        protected void WordForms(string word, string[] endings)
        {
            WordForms(word, endings, 0);
        }

        /// <summary>
        ///     Установить имя человека
        /// </summary>
        /// <param name="name">Имя</param>
        /// <returns></returns>
        public Core SetFirstName(string name)
        {
            if (name.Trim() != "")
            {
                var tmpWord = new Word(name);
                tmpWord.NamePart = NamePart.FirstName;
                words.AddWord(tmpWord);
                NotReady();
            }

            return this;
        }

        /// <summary>
        ///     Установить фамилию человека
        /// </summary>
        /// <param name="name">Фамилия</param>
        /// <returns></returns>
        public Core SetSecondName(string name)
        {
            if (name.Trim() != "")
            {
                var tmpWord = new Word(name);
                tmpWord.NamePart = NamePart.SecondName;
                words.AddWord(tmpWord);
                NotReady();
            }

            return this;
        }

        /// <summary>
        ///     Установить отчество человека
        /// </summary>
        /// <param name="name">Отчество</param>
        /// <returns></returns>
        public Core SetFatherName(string name)
        {
            if (name.Trim() != "")
            {
                var tmpWord = new Word(name);
                tmpWord.NamePart = NamePart.FatherName;
                words.AddWord(tmpWord);
                NotReady();
            }

            return this;
        }

        /// <summary>
        ///     Устанавливает пол человека
        /// </summary>
        /// <param name="gender">Пол человека</param>
        /// <returns></returns>
        public Core SetGender(Gender gender)
        {
            var length = words.Length;

            for (var i = 0; i < length; i++)
            {
                words.GetWord(i).Gender = gender;
            }

            return this;
        }

        /// <summary>
        ///     Устанавливает полное ФИО
        /// </summary>
        /// <param name="secondName">Фамилия</param>
        /// <param name="firstName">Имя</param>
        /// <param name="fatherName">Отчество</param>
        /// <returns></returns>
        public Core SetFullName(string secondName, string firstName, string fatherName)
        {
            SetFirstName(firstName);
            SetSecondName(secondName);
            SetFatherName(fatherName);

            return this;
        }

        /// <summary>
        ///     Установить имя человека
        /// </summary>
        /// <param name="name">Имя</param>
        /// <returns></returns>
        public Core SetName(string name)
        {
            return SetFirstName(name);
        }

        /// <summary>
        ///     Установить фамилию человека
        /// </summary>
        /// <param name="name">Фамилия</param>
        /// <returns></returns>
        public Core SetLastName(string name)
        {
            return SetSecondName(name);
        }

        /// <summary>
        ///     Установить фамилию человека
        /// </summary>
        /// <param name="name">Фамилия</param>
        /// <returns></returns>
        public Core SetSirName(string name)
        {
            return SetSecondName(name);
        }

        /// <summary>
        ///     Идентификирует нужное слово
        /// </summary>
        /// <param name="word">Слово</param>
        private void PrepareNamePart(Word word)
        {
            if (word.NamePart == NamePart.Null)
            {
                DetectNamePart(word);
            }
        }

        /// <summary>
        ///     Идентифицирует все существующие слова
        /// </summary>
        private void PrepareAllNameParts()
        {
            var length = words.Length;

            for (var i = 0; i < length; i++)
            {
                PrepareNamePart(words.GetWord(i));
            }
        }

        /// <summary>
        ///     Предварительно определяет пол во слове
        /// </summary>
        /// <param name="word">Слово для определения</param>
        private void PrepareGender(Word word)
        {
            if (!word.isGenderSolved())
            {
                switch (word.NamePart)
                {
                    case NamePart.FirstName:
                        GenderByFirstName(word);

                        break;
                    case NamePart.SecondName:
                        GenderBySecondName(word);

                        break;
                    case NamePart.FatherName:
                        GenderByFatherName(word);

                        break;
                }
            }
        }

        /// <summary>
        ///     Принимает решение о текущем поле человека
        /// </summary>
        private void SolveGender()
        {
            //Ищем, может гдето пол уже установлен
            var length = words.Length;

            for (var i = 0; i < length; i++)
            {
                if (words.GetWord(i).isGenderSolved())
                {
                    SetGender(words.GetWord(i).Gender);

                    return;
                }
            }

            //Если нет тогда определяем у каждого слова и потом сумируем
            var probability = new GenderProbability(0, 0);

            for (var i = 0; i < length; i++)
            {
                var word = words.GetWord(i);
                PrepareGender(word);
                probability = probability + word.GenderProbability;
            }

            if (probability.Man > probability.Woman)
            {
                SetGender(Gender.Man);
            }
            else
            {
                SetGender(Gender.Woman);
            }
        }

        /// <summary>
        ///     Выполнет все необходимые подготовления для склонения.
        ///     Все слова идентфицируются. Определяется пол.
        /// </summary>
        private void PrepareEverything()
        {
            if (!ready)
            {
                PrepareAllNameParts();
                SolveGender();
                ready = true;
            }
        }

        /// <summary>
        ///     По указаным словам определяется пол человека
        /// </summary>
        /// <returns>Пол человека</returns>
        public Gender GenderAutoDetect()
        {
            PrepareEverything();

            if (words.Length > 0)
            {
                return words.GetWord(0).Gender;
            }

            return Gender.Null;
        }

        /// <summary>
        ///     Разделяет слова на части и готовит к подальшуму склонению
        /// </summary>
        /// <param name="fullname">Строка котороя содержит полное имя</param>
        private void SplitFullName(string fullname)
        {
            var arr = fullname.Trim().Split(' ');
            var length = arr.Length;

            words = new WordArray();

            for (var i = 0; i < length; i++)
            {
                if (arr[i] != "")
                {
                    words.AddWord(new Word(arr[i]));
                }
            }
        }

        /// <summary>
        ///     Метод в разработке
        /// </summary>
        /// <returns></returns>
        public string GetFullNameFormat___DEV()
        {
            return "";
        }

        /// <summary>
        ///     Склоняет слово по нужным правилам
        /// </summary>
        /// <param name="word">Слово</param>
        protected void WordCase(Word word)
        {
            var gender = word.Gender;
            var genderName = gender == Gender.Man ? "Man" : "Woman";

            var namePartName = "";

            switch (word.NamePart)
            {
                case NamePart.FirstName:
                    namePartName = "First";

                    break;
                case NamePart.SecondName:
                    namePartName = "Second";

                    break;
                case NamePart.FatherName:
                    namePartName = "Father";

                    break;
            }

            var methodName = genderName + namePartName + "Name";
            SetWorkingWord(word.Name);

            var res = (bool) GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(this, null);

            if (res)
            {
                word.NameCases = lastResult;
                word.Rule = lastRule;
            }
            else
            {
                MakeResultTheSame();
                word.NameCases = lastResult;
                word.Rule = -1;
            }
        }

        /// <summary>
        ///     Производит склонение всех слов
        /// </summary>
        private void AllWordCases()
        {
            if (!finished)
            {
                PrepareEverything();
                var length = words.Length;

                for (var i = 0; i < length; i++)
                {
                    WordCase(words.GetWord(i));
                }

                finished = true;
            }
        }

        /// <summary>
        ///     Возвращает масив который содержит все падежи имени
        /// </summary>
        /// <returns>Возвращает массив со всеми падежами имени</returns>
        public string[] GetFirstNameCase()
        {
            AllWordCases();

            return words.GetByNamePart(NamePart.FirstName).NameCases;
        }

        /// <summary>
        ///     Возвращает имя в определенном падеже
        /// </summary>
        /// <param name="caseNum">Падеж</param>
        /// <returns>Имя в определенном падеже</returns>
        public string GetFirstNameCase(Padeg caseNum)
        {
            AllWordCases();

            return words.GetByNamePart(NamePart.FirstName).GetNameCase(caseNum);
        }

        /// <summary>
        ///     Возвращает масив который содержит все падежи фамилии
        /// </summary>
        /// <returns>Возвращает массив со всеми падежами фамилии</returns>
        public string[] GetSecondNameCase()
        {
            AllWordCases();

            return words.GetByNamePart(NamePart.SecondName).NameCases;
        }

        /// <summary>
        ///     Возвращает фамилию в определенном падеже
        /// </summary>
        /// <param name="caseNum">Падеж</param>
        /// <returns>Фамилия в определенном падеже</returns>
        public string GetSecondNameCase(Padeg caseNum)
        {
            AllWordCases();

            return words.GetByNamePart(NamePart.SecondName).GetNameCase(caseNum);
        }

        /// <summary>
        ///     Возвращает масив который содержит все падежи отчества
        /// </summary>
        /// <returns>Возвращает массив со всеми падежами отчества</returns>
        public string[] GetFatherNameCase()
        {
            AllWordCases();

            return words.GetByNamePart(NamePart.FatherName).NameCases;
        }

        /// <summary>
        ///     Возвращает отчество в определенном падеже
        /// </summary>
        /// <param name="caseNum">Падеж</param>
        /// <returns>Отчество в определенном падеже</returns>
        public string GetFatherNameCase(Padeg caseNum)
        {
            AllWordCases();

            return words.GetByNamePart(NamePart.FatherName).GetNameCase(caseNum);
        }

        /// <summary>
        ///     Выполняет склонение имени
        /// </summary>
        /// <param name="firstName">Имя</param>
        /// <param name="gender">Пол</param>
        /// <returns>Массив со всеми падежами</returns>
        public string[] QFirstName(string firstName, Gender gender)
        {
            FullReset();
            SetFirstName(firstName);

            if (gender != Gender.Null)
            {
                SetGender(gender);
            }

            return GetFirstNameCase();
        }

        /// <summary>
        ///     Выполняет склонение имени
        /// </summary>
        /// <param name="firstName">Имя</param>
        /// <returns>Массив со всеми падежами</returns>
        public string[] QFirstName(string firstName)
        {
            return QFirstName(firstName, Gender.Null);
        }

        /// <summary>
        ///     Выполняет склонение имени
        /// </summary>
        /// <param name="firstName">Имя</param>
        /// <param name="caseNum">Падеж</param>
        /// <param name="gender">Пол</param>
        /// <returns>Имя в указаном падеже</returns>
        public string QFirstName(string firstName, Padeg caseNum, Gender gender)
        {
            FullReset();
            SetFirstName(firstName);

            if (gender != Gender.Null)
            {
                SetGender(gender);
            }

            return GetFirstNameCase(caseNum);
        }

        /// <summary>
        ///     Выполняет склонение имени
        /// </summary>
        /// <param name="firstName">Имя</param>
        /// <param name="caseNum">Падеж</param>
        /// <returns>Имя в указаном падеже</returns>
        public string QFirstName(string firstName, Padeg caseNum)
        {
            return QFirstName(firstName, caseNum, Gender.Null);
        }

        /// <summary>
        ///     Выполняет склонение фамилии
        /// </summary>
        /// <param name="name">Фамилия</param>
        /// <param name="gender">Пол</param>
        /// <returns>Массив со всеми падежами</returns>
        public string[] QSecondName(string name, Gender gender)
        {
            FullReset();
            SetSecondName(name);

            if (gender != Gender.Null)
            {
                SetGender(gender);
            }

            return GetSecondNameCase();
        }

        /// <summary>
        ///     Выполняет склонение фамилии
        /// </summary>
        /// <param name="name">Фамилия</param>
        /// <returns>Массив со всеми падежами</returns>
        public string[] QSecondName(string name)
        {
            return QSecondName(name, Gender.Null);
        }

        /// <summary>
        ///     Выполняет склонение фамилии
        /// </summary>
        /// <param name="name">Фамилия</param>
        /// <param name="caseNum">Падеж</param>
        /// <param name="gender">Пол</param>
        /// <returns>Фамилия в указаном падеже</returns>
        public string QSecondName(string name, Padeg caseNum, Gender gender)
        {
            FullReset();
            SetSecondName(name);

            if (gender != Gender.Null)
            {
                SetGender(gender);
            }

            return GetSecondNameCase(caseNum);
        }

        /// <summary>
        ///     Выполняет склонение фамилии
        /// </summary>
        /// <param name="name">Фамилия</param>
        /// <param name="caseNum">Падеж</param>
        /// <returns>Фамилия в указаном падеже</returns>
        public string QSecondName(string name, Padeg caseNum)
        {
            return QSecondName(name, caseNum, Gender.Null);
        }

        /// <summary>
        ///     Выполняет склонение фамилии
        /// </summary>
        /// <param name="name">Фамилия</param>
        /// <param name="gender">Пол</param>
        /// <returns>Массив со всеми падежами</returns>
        public string[] QFatherName(string name, Gender gender)
        {
            FullReset();
            SetFatherName(name);

            if (gender != Gender.Null)
            {
                SetGender(gender);
            }

            return GetFatherNameCase();
        }

        /// <summary>
        ///     Выполняет склонение фамилии
        /// </summary>
        /// <param name="name">Фамилия</param>
        /// <returns>Массив со всеми падежами</returns>
        public string[] QFatherName(string name)
        {
            return QFatherName(name, Gender.Null);
        }

        /// <summary>
        ///     Выполняет склонение отчества
        /// </summary>
        /// <param name="name">Отчество</param>
        /// <param name="caseNum">Падеж</param>
        /// <param name="gender">Пол</param>
        /// <returns>Отчество в указаном падеже</returns>
        public string QFatherName(string name, Padeg caseNum, Gender gender)
        {
            FullReset();
            SetFatherName(name);

            if (gender != Gender.Null)
            {
                SetGender(gender);
            }

            return GetFatherNameCase(caseNum);
        }

        /// <summary>
        ///     Выполняет склонение фамилии
        /// </summary>
        /// <param name="name">Фамилия</param>
        /// <param name="caseNum">Падеж</param>
        /// <returns>Фамилия в указаном падеже</returns>
        public string QFatherName(string name, Padeg caseNum)
        {
            return QFatherName(name, caseNum, Gender.Null);
        }

        /// <summary>
        ///     Соединяет все слова которые есть в системе в одну строку в определенном падеже
        /// </summary>
        /// <param name="caseNum">Падеж</param>
        /// <returns>Соедененая строка</returns>
        private string ConnectedCase(Padeg caseNum)
        {
            var length = words.Length;
            var result = "";

            for (var i = 0; i < length; i++)
            {
                result += words.GetWord(i).GetNameCase(caseNum) + " ";
            }

            return result.TrimEnd();
        }

        /// <summary>
        ///     Соединяет все слова которые есть в системе в массив со всеми падежами
        /// </summary>
        /// <returns>Массив со всеми падежами</returns>
        private string[] ConnectedCases()
        {
            var res = new string[caseCount];

            for (var i = 0; i < caseCount; i++)
            {
                res[i] = ConnectedCase((Padeg) i);
            }

            return res;
        }

        /// <summary>
        ///     Выполняет склонение полного имени
        /// </summary>
        /// <param name="fullName">Полное имя</param>
        /// <param name="gender">Пол</param>
        /// <returns>Массив со всеми падежами</returns>
        public string[] Q(string fullName, Gender gender)
        {
            FullReset();
            SplitFullName(fullName);

            if (gender != Gender.Null)
            {
                SetGender(gender);
            }

            AllWordCases();

            return ConnectedCases();
        }

        /// <summary>
        ///     Выполняет склонение полного имени
        /// </summary>
        /// <param name="fullName">Полное имя</param>
        /// <returns>Массив со всеми падежами</returns>
        public string[] Q(string fullName)
        {
            return Q(fullName, Gender.Null);
        }

        /// <summary>
        ///     Выполняет склонение полного имени
        /// </summary>
        /// <param name="fullName">Полное имя</param>
        /// <param name="caseNum">Падеж</param>
        /// <param name="gender">Пол</param>
        /// <returns>Строка в указаном падеже</returns>
        public string Q(string fullName, Padeg caseNum, Gender gender)
        {
            FullReset();
            SplitFullName(fullName);

            if (gender != Gender.Null)
            {
                SetGender(gender);
            }

            AllWordCases();

            return ConnectedCase(caseNum);
        }

        /// <summary>
        ///     Выполняет склонение полного имени
        /// </summary>
        /// <param name="fullName">Полное имя</param>
        /// <param name="caseNum">Падеж</param>
        /// <returns>Строка в указаном падеже</returns>
        public string Q(string fullName, Padeg caseNum)
        {
            return Q(fullName, caseNum, Gender.Null);
        }

        /// <summary>
        ///     Возвращает массив всех слов
        /// </summary>
        /// <returns>Массив всех слов</returns>
        public WordArray GetWordsArray()
        {
            return words;
        }

        /// <summary>
        ///     Склонение имени по правилам мужских имен
        /// </summary>
        /// <returns>true если склонение было произведено и false если правило не было найденым</returns>
        protected abstract bool ManFirstName();

        /// <summary>
        ///     Склонение имени по правилам женских имен
        /// </summary>
        /// <returns>true если склонение было произведено и false если правило не было найденым</returns>
        protected abstract bool WomanFirstName();

        /// <summary>
        ///     Склонение фамилию по правилам мужских имен
        /// </summary>
        /// <returns>true если склонение было произведено и false если правило не было найденым</returns>
        protected abstract bool ManSecondName();

        /// <summary>
        ///     Склонение фамилию по правилам женских имен
        /// </summary>
        /// <returns>true если склонение было произведено и false если правило не было найденым</returns>
        protected abstract bool WomanSecondName();

        /// <summary>
        ///     Склонение отчества по правилам мужских имен
        /// </summary>
        /// <returns>true если склонение было произведено и false если правило не было найденым</returns>
        protected abstract bool ManFatherName();

        /// <summary>
        ///     Склонение отчества по правилам женских имен
        /// </summary>
        /// <returns>true если склонение было произведено и false если правило не было найденым</returns>
        protected abstract bool WomanFatherName();

        /// <summary>
        ///     Определяет пол человека по его имени
        /// </summary>
        /// <param name="word">Имя</param>
        protected abstract void GenderByFirstName(Word word);

        /// <summary>
        ///     Определяет пол человека по его фамилии
        /// </summary>
        /// <param name="word">Фамилия</param>
        protected abstract void GenderBySecondName(Word word);

        /// <summary>
        ///     Определяет пол человека по его отчеству
        /// </summary>
        /// <param name="word">Отчество</param>
        protected abstract void GenderByFatherName(Word word);

        /// <summary>
        ///     Идетифицирует слово определяе имя это, или фамилия, или отчество
        /// </summary>
        /// <param name="word">Слово для которое нужно идетифицировать</param>
        protected abstract void DetectNamePart(Word word);
    }
}