using System;
using System.Collections.Generic;
using System.Linq;

/////////////////////////////////////////////////////////////////////////////
// author : CretaPark (creta5164@gmail.com)
// SimpleAutomata.cs
/////////////////////////////////////////////////////////////////////////////

namespace UInput
{
    public class SimpleAutomata
    {
        public const char UNDERBAR_IDLE = '＿';

        private enum EndUpStyle { none, endUp, alone }
        
        /// <summary>
        /// 자음, 모음 집합을 구현하기 위한 구조체입니다.
        /// </summary>
        private struct GlyphInfo {
            /// <summary>
            /// 0x3131로부터 상대적인 위치
            /// </summary>
            public uint index;

            /// <summary>
            /// 범위를 기준으로 하는 상대위치
            /// </summary>
            public uint globalIndex;

            /// <summary>
            /// 조립이 가능한 글자 집합
            /// </summary>
            public string constructGlyphs;

            public EndUpStyle endUpStyle;

            public GlyphInfo(uint index, uint globalIndex)
            {
                this.index = index;
                this.globalIndex = globalIndex;
                constructGlyphs = null;
                endUpStyle = EndUpStyle.none;
            }

            public GlyphInfo(uint index, uint globalIndex, EndUpStyle endUpStyle)
            {
                this.index = index;
                this.globalIndex = globalIndex;
                constructGlyphs = null;
                this.endUpStyle = endUpStyle;
            }

            public GlyphInfo(uint index, uint globalIndex, string constructGlyphs)
            {
                this.index = index;
                this.globalIndex = globalIndex;
                this.constructGlyphs = constructGlyphs;
                endUpStyle = EndUpStyle.none;
            }
        }

        private enum AutomataScope { idle, first, second, last }

        /// <summary>
        /// 현재 오토마타를 보여줍니다.
        /// </summary>
        public static char buildResult { get; private set; }
        
        /// <summary>
        /// 현재 오토마타가 조합되고 있는 상태인 지 알려줍니다.
        /// </summary>
        public static bool isBuilding { get {
                return characterHistory.Count != 0;
            } }

        /// <summary>
        /// 오토마타를 포함하지 않는 현재 만들어진 텍스트입니다.
        /// </summary>
        public static string currentText = "";

        /// <summary>
        /// 오토마타가 초성, 중성, 종성인 지 가리키는 플래그입니다.
        /// </summary>
        private static AutomataScope currentScope;

        /// <summary>
        /// 오토마타 상태에서 되돌리기를 사용할 때 이용하는 목록입니다.
        /// </summary>
        private static List<char> characterHistory      = new List<char>();

        /// <summary>
        /// 오토마타 상태에서 되돌리기를 사용할 때 이용하는 목록입니다.
        /// </summary>
        private static List<AutomataScope> scopeHistory = new List<AutomataScope>();

        /// <summary>
        /// 자음 집합입니다.
        /// </summary>
        private static readonly Dictionary<char, GlyphInfo> Consonant = new Dictionary<char, GlyphInfo>() {
            { 'ㄱ',     new GlyphInfo(0,  0  , "ㄱㅅ"          )},
                { 'ㄲ', new GlyphInfo(1,  1                    )},
                { 'ㄳ', new GlyphInfo(2,  0                    )},
            { 'ㄴ',     new GlyphInfo(3,  2  , "ㅈㅎ"          )},
                { 'ㄵ', new GlyphInfo(4,  0                    )},
                { 'ㄶ', new GlyphInfo(5,  0                    )},
            { 'ㄷ',     new GlyphInfo(6,  3  , "ㄷ"            )},
                { 'ㄸ', new GlyphInfo(7,  4  , EndUpStyle.endUp)},
            { 'ㄹ',     new GlyphInfo(8,  5  , "ㄱㅁㅂㅅㅌㅍㅎ")},
                { 'ㄺ', new GlyphInfo(9,  0                    )},
                { 'ㄻ', new GlyphInfo(10, 0                    )},
                { 'ㄼ', new GlyphInfo(11, 0                    )},
                { 'ㄽ', new GlyphInfo(12, 0                    )},
                { 'ㄾ', new GlyphInfo(13, 0                    )},
                { 'ㄿ', new GlyphInfo(14, 0                    )},
                { 'ㅀ', new GlyphInfo(15, 0                    )},
            { 'ㅁ',     new GlyphInfo(16, 6                    )},
            { 'ㅂ',     new GlyphInfo(17, 7  , "ㅂㅅ"          )},
                { 'ㅃ', new GlyphInfo(18, 8  , EndUpStyle.endUp)},
                { 'ㅄ', new GlyphInfo(19, 0                    )},
            { 'ㅅ',     new GlyphInfo(20, 9  , "ㅅ"            )},
                { 'ㅆ', new GlyphInfo(21, 10                   )},
            { 'ㅇ',     new GlyphInfo(22, 11 , EndUpStyle.alone)},
            { 'ㅈ',     new GlyphInfo(23, 12 , "ㅈ"            )},
                { 'ㅉ', new GlyphInfo(24, 13 , EndUpStyle.endUp)},
            { 'ㅊ',     new GlyphInfo(25, 14 , EndUpStyle.alone)},
            { 'ㅋ',     new GlyphInfo(26, 15 , EndUpStyle.alone)},
            { 'ㅌ',     new GlyphInfo(27, 16 , EndUpStyle.alone)},
            { 'ㅍ',     new GlyphInfo(28, 17 , EndUpStyle.alone)},
            { 'ㅎ',     new GlyphInfo(29, 18 , EndUpStyle.alone)}
        };

        /// <summary>
        /// 모음 집합입니다.
        /// </summary>
        private static readonly Dictionary<char, GlyphInfo> Vowel = new Dictionary<char, GlyphInfo>() {
            { 'ㅏ',     new GlyphInfo(0,  0, "ㅣㅏ"    )},
                { 'ㅐ', new GlyphInfo(1,  0            )},
            { 'ㅑ',     new GlyphInfo(2,  1, "ㅣ"      )},
                { 'ㅒ', new GlyphInfo(3,  0            )},
            { 'ㅓ',     new GlyphInfo(4,  2, "ㅣㅓ"    )},
                { 'ㅔ', new GlyphInfo(5,  0            )},
            { 'ㅕ',     new GlyphInfo(6,  3, "ㅣ"      )},
                { 'ㅖ', new GlyphInfo(7,  0            )},
            { 'ㅗ',     new GlyphInfo(8,  4, "ㅏㅐㅣㅗ")},
                { 'ㅘ', new GlyphInfo(9,  0, "ㅣ"      )},
                { 'ㅙ', new GlyphInfo(10, 0            )},
                { 'ㅚ', new GlyphInfo(11, 0            )},
            { 'ㅛ',     new GlyphInfo(12, 5            )},
            { 'ㅜ',     new GlyphInfo(13, 6, "ㅓㅔㅣㅜ")},
                { 'ㅝ', new GlyphInfo(14, 0, "ㅣ"      )},
                { 'ㅞ', new GlyphInfo(15, 0            )},
                { 'ㅟ', new GlyphInfo(16, 0            )},
            { 'ㅠ',     new GlyphInfo(17, 7            )},
            { 'ㅡ',     new GlyphInfo(18, 8, "ㅣ"      )},
                { 'ㅢ', new GlyphInfo(19, 0            )},
            { 'ㅣ',     new GlyphInfo(20, 9            )}
        };

        /// <summary>
        /// 오토마타로 만들어지는 글리프를 되돌리거나 텍스트에서 문자를 제거합니다.
        /// </summary>
        public static bool UndoAutomata(){
            buildResult = UNDERBAR_IDLE;
            if (characterHistory.Count > 0) {
                if (Consonant.ContainsKey(characterHistory[characterHistory.Count - 1])) {
                    uint glyph = characterHistory[characterHistory.Count - 1];
                    GlyphInfo data = Consonant[Convert.ToChar(glyph)];
                    if (data.constructGlyphs == null &&
                        data.endUpStyle != EndUpStyle.alone) {
                        while (data.constructGlyphs == null)
                            data = Consonant[Convert.ToChar(--glyph)];

                        characterHistory[characterHistory.Count - 1] = Convert.ToChar(glyph);

                        buildResult = BuildGlyph(characterHistory);

                        return true;
                    }

                } else if (Vowel.ContainsKey(characterHistory[characterHistory.Count - 1])) {
                    uint glyph = characterHistory[characterHistory.Count - 1];
                    GlyphInfo data = Vowel[Convert.ToChar(glyph)];
                    if (data.constructGlyphs == null) {
                        while (data.constructGlyphs == null || 
                              (data.constructGlyphs != null && data.constructGlyphs.Length <= 1))
                            data = Vowel[Convert.ToChar(--glyph)];

                        characterHistory[characterHistory.Count - 1] = Convert.ToChar(glyph);

                        buildResult = BuildGlyph(characterHistory);

                        return true;
                    }
                }

                characterHistory.RemoveAt(characterHistory.Count - 1);
                scopeHistory.RemoveAt(scopeHistory.Count - 1);
                currentScope = scopeHistory.Count > 0 ? scopeHistory[scopeHistory.Count - 1] : AutomataScope.idle;
                buildResult = BuildGlyph(characterHistory);
            } else if (currentText.Length > 1) {
                currentText = currentText.Remove(currentText.Length - 1);
            } else
                currentText = "";

            return false;
        }
        
        /// <summary>
        /// 자음 또는 모음인 글자를 오토마타에 적용합니다.
        /// </summary>
        /// <param name="glyph">자음 또는 모음인 글자</param>
        /// <exception cref="Exception">글자가 자음/모음이 아닌 문자</exception>
        public static void PushAutomata(char glyph)
        {
            if (glyph == ' ' || glyph == '_' || glyph == '＿') {
                if (characterHistory.Count != 0)
                    EscapeGlyph();
                else
                    currentText += " ";
                return;
            }

            uint scope = Convert.ToUInt32(glyph);

            if (scope < 0x3131 || scope > 0x3163)
                throw new Exception("악");

            scope -= 0x3131;

            uint cacheGlyph;

            switch (currentScope) {
                default:
                case AutomataScope.idle:
                    if (Consonant.ContainsKey(glyph)) {

                        characterHistory.Clear();
                        scopeHistory.Clear();
                        characterHistory.Add(glyph);
                        scopeHistory.Add(currentScope = AutomataScope.first);
                    }
                    break;

                case AutomataScope.first:
                    if (characterHistory.Count != 1)
                        goto default;

                    if (Vowel.ContainsKey(glyph)) {

                        characterHistory.Add(glyph);
                        scopeHistory.Add(currentScope = AutomataScope.second);

                    } else if (Consonant.ContainsKey(glyph)) {
                        if (Consonant[characterHistory[0]]
                            .constructGlyphs == null) {

                            EscapeGlyph(glyph);

                        } else if (Consonant[characterHistory[0]]
                            .constructGlyphs.Contains(glyph)) {

                            cacheGlyph = Convert.ToUInt32(characterHistory[0])
                                       + (uint)Consonant[characterHistory[0]]
                                               .constructGlyphs.IndexOf(glyph)
                                               + 1;

                            characterHistory[0] = Convert.ToChar(cacheGlyph);

                        } else {

                            EscapeGlyph(glyph);
                        }
                    } else {

                        EscapeGlyph(glyph);

                    }

                    break;

                case AutomataScope.second:
                    if (characterHistory.Count != 2)
                        goto default;

                    if (Consonant.ContainsKey(glyph)) {

                        characterHistory.Add(glyph);
                        scopeHistory.Add(currentScope = AutomataScope.last);

                    } else if (Vowel.ContainsKey(glyph)) {
                        if (Vowel[characterHistory[1]]
                            .constructGlyphs == null) {

                            EscapeGlyph(glyph);

                        } else if (Vowel[characterHistory[1]]
                            .constructGlyphs.Contains(glyph)) {

                            cacheGlyph = Convert.ToUInt32(characterHistory[1])
                                       + (uint)Vowel[characterHistory[1]]
                                               .constructGlyphs.IndexOf(glyph)
                                               + 1;

                            characterHistory[1] = Convert.ToChar(cacheGlyph);

                        } else {

                            EscapeGlyph(glyph);
                        }
                    } else {

                        EscapeGlyph(glyph);

                    }

                    break;

                case AutomataScope.last:
                    if (characterHistory.Count != 3)
                        goto default;

                    if (Vowel.ContainsKey(glyph)) {

                        EscapeGlyph(glyph);

                    } else if (Consonant.ContainsKey(glyph)) {
                        if (Consonant[characterHistory[2]]
                            .constructGlyphs == null) {

                            EscapeGlyph(glyph);

                        } else if (Consonant[characterHistory[2]]
                                  .constructGlyphs.Contains(glyph)) {

                            cacheGlyph = Convert.ToUInt32(characterHistory[2])
                                       + (uint)Consonant[characterHistory[2]]
                                               .constructGlyphs.IndexOf(glyph)
                                       + 1;

                            if (Consonant[Convert.ToChar(cacheGlyph)].endUpStyle
                                == EndUpStyle.endUp) {
                                UndoAutomata();
                                EscapeGlyph();

                                characterHistory.Clear();
                                characterHistory.Add(Convert.ToChar(cacheGlyph));
                                scopeHistory.Clear();
                                scopeHistory.Add(currentScope = AutomataScope.first);
                            } else
                                characterHistory[2] = Convert.ToChar(cacheGlyph);
                            
                        } else {

                            EscapeGlyph(glyph);
                        }
                    }
                    break;
            }

            buildResult = BuildGlyph(characterHistory);
        }
        
        [Obsolete("사용 안할 지 고민 중...")]
        public static void PushAutomata(string glyph)
            => PushAutomata(glyph[0]);

        /// <summary>
        /// 오토마타 스코프에 잡힌 문자에서 탈출합니다.
        /// </summary>
        public static void EscapeGlyph()
        {
            char build = BuildGlyph(characterHistory);
            if (build != '\0')
                currentText += build;
            characterHistory.Clear();
            scopeHistory.Clear();
            scopeHistory.Add(currentScope = AutomataScope.idle);

            buildResult = UNDERBAR_IDLE;
        }

        /// <summary>
        /// 오토마타 스코프 글자에서 만약 <paramref name="castChar"/>이 모음이거나 이중 자음이지만,
        /// <para>한 글자에 있는 것이 불가능한 경우, 마지막 문자를 가져와 다음 오토마타를 만들어 붙이고 탈출합니다.</para>
        /// </summary>
        /// <param name="castChar">다음 오토마타에 붙일 새 문자</param>
        private static void EscapeGlyph(char castChar)
        {
            //if (characterHistory.Count <= 2) return;

            if (Vowel.ContainsKey(castChar)) {
                char lastIndex = characterHistory[characterHistory.Count - 1];
                if (UndoAutomata()) {
                    GlyphInfo data = Consonant[characterHistory[characterHistory.Count - 1]];

                    lastIndex = data.constructGlyphs[(int)Consonant[lastIndex].index - (int)data.index - 1];
                }
                
                EscapeGlyph();
                characterHistory.Add(lastIndex);

                scopeHistory.Add(currentScope = AutomataScope.first);
            } else {
                EscapeGlyph();
                scopeHistory.Add(currentScope = AutomataScope.idle);
            }
            PushAutomata(castChar);
        }

        /// <summary>
        /// 오토마타에서 글자로 조립합니다.
        /// </summary>
        /// <param name="characterHistory">조립 할 오토마타 기록 리스트</param>
        /// <returns></returns>
        private static char BuildGlyph(List<char> characterHistory)
        {
            if (characterHistory.Count == 0)
                return '\0';

            if (characterHistory.Count == 1)
                return characterHistory[0];

            uint glyph = 0xAC00
                       + 0x24C
                       * Consonant[characterHistory[0]].globalIndex;
            
                glyph += 0x1C
                       * Vowel[characterHistory[1]].index;

            if (characterHistory.Count == 2)
                return Convert.ToChar(glyph);

            uint indexValue = Consonant[characterHistory[2]].index;

            if (indexValue > Consonant['ㅉ'].index)
                indexValue -= 3;
            else if (indexValue > Consonant['ㅃ'].index)
                indexValue -= 2;
            else if (indexValue > Consonant['ㄸ'].index)
                indexValue--;

            glyph += indexValue + 1;

            return Convert.ToChar(glyph);
        }

    }
}
