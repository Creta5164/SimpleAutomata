# SimpleAutomata
단순한 한글 자음/모음 조합을 돕는 클래스입니다.

## 사용법
`Consonant`와 `Vowel` 안의 목록대로, <u>들여쓰기가 되지 않은 문자</u>가 입력 가능한 문자들입니다.<br>
클래스를 파악하지 못했다면, 이 안의 내용들을 수정하는 것을 권장하지 않습니다.

### 글자 입력
```csharp
SimpleAutomata.PushAutomata(char <자음/모음인 한글 또는 공백>);
```
> **오토마타에 글자를 입력**합니다. 공백 문자는 띄어쓰기 말고도, **조합중인 문자에서 탈출할 수 있습니다.**
> 결과물은 `SimpleAutomata.buildResult`에서 상태를 확인할 수 있고, 조합이 되는 문자열은 `SimpleAutomata.currentText` 를 통해 접근할 수 있습니다.


### 글자 삭제
```csharp
SimpleAutomata.UndoAutomata();
```
> 오토마타로 조합되고 있는 문자는, 입력한 문자 순서대로 되돌리며, 조합중인 상태가 아니면, `SimpleAutomata.currentText`의 마지막 문자를 하나씩 제거합니다.


### 조합 탈출
```csharp
SimpleAutomata.EscapeGlyph();
```
> 오토마타로 조합되고 있는 문자에서 탈출합니다.


지원되는 입력
- 자음/모음, 기본 한글입력
- 쌍자음/모음 입력
- 조합 불가 문자 탈출
