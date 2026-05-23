using UnityEngine;
using UnityEngine.UI;

public static class DialogueButtonStyling
{
    public static void ApplyChoiceButtonHover(Button button)
    {
        if (button == null)
            return;

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 1f);
        colors.highlightedColor = new Color(0.7f, 0.7f, 0.75f, 1f);
        colors.pressedColor = new Color(0.4f, 0.4f, 0.45f, 1f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
    }
}
