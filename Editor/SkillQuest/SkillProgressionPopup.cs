using System.Diagnostics;
using ImGuiNET;
using SharpDX;
using T3.Core.Utils;
using T3.Editor.Gui;
using T3.Editor.Gui.Input;
using T3.Editor.Gui.Styling;
using T3.Editor.Gui.UiHelpers;
using T3.Editor.SkillQuest.Data;
using Color = T3.Core.DataTypes.Vector.Color;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace T3.Editor.SkillQuest;

/// <summary>
/// A dialog that is shown after level completed
/// </summary>
internal static class SkillProgressionPopup
{
    internal static void Draw()
    {
        var windowSize = new Vector2(600, 260) * T3Ui.UiScaleFactor;

        // Center the popup in the main viewport
        var vp = ImGui.GetMainViewport();
        var pos = vp.Pos + (vp.Size - windowSize) * 0.5f;
        ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);
        ImGui.SetNextWindowPos(pos);

        if (!SkillManager.TryGetActiveTopicAndLevel(out var topic, out var previousLevel))
            return;

        ImGui.PushStyleColor(ImGuiCol.PopupBg, UiColors.BackgroundFull.Rgba);
        if (ImGui.BeginPopup("ProgressionPopup", ImGuiWindowFlags.NoResize |
                                                 ImGuiWindowFlags.NoMove |
                                                 ImGuiWindowFlags.Modal))
        {
            var index = topic.Levels.IndexOf(previousLevel);
            Debug.Assert(index >= 0);

            if (index < topic.Levels.Count - 1)
            {
                var nextLevel = topic.Levels[index + 1];
                DrawNextLevelContent(topic, previousLevel, nextLevel, index);
            }

            DrawActionBar();

            ImGui.EndPopup();
        }

        ImGui.PopStyleColor();
    }

    private static void DrawNextLevelContent(QuestTopic topic, QuestLevel previousLevel, QuestLevel nextLevel, int index)
    {
        var uiScale = T3Ui.UiScaleFactor;
        var dl = ImGui.GetWindowDrawList();

        ImGui.BeginChild("UpperArea", new Vector2(0, -30 * uiScale), false, ImGuiWindowFlags.NoBackground);
        {
            var area = ImRect.RectWithSize(ImGui.GetWindowPos(), ImGui.GetWindowSize());
            area.Expand(-10);
            dl.AddRectFilled(area.Min, area.Max, UiColors.WindowBackground, 7 * uiScale);

            var leftWidth = 180 * uiScale;
            ImGui.BeginChild("Left", new Vector2(leftWidth, 0), false, ImGuiWindowFlags.NoBackground);
            {
                var donutSize = 140 * uiScale;

                // center the donut in the left column
                var cp = ImGui.GetCursorPos();
                cp.X += (leftWidth - donutSize) * 0.5f;
                cp.Y += 10 * uiScale;
                ImGui.SetCursorPos(cp);

                var torusCenter = ImGui.GetCursorScreenPos() + new Vector2(100, 120);
                var progress = (index + 1f) / topic.Levels.Count;
                DrawTorusProgress(dl, torusCenter, 100, 1, UiColors.BackgroundFull.Fade(0.6f));
                DrawTorusProgress(dl, torusCenter, 100, progress, UiColors.StatusActivated);

                ImGui.SetCursorPos(cp + new Vector2(0, donutSize * 0.5f - Fonts.FontNormal.FontSize * 0.5f));
                ImGui.PushFont(Fonts.FontLarge);
                CenteredText($"{index + 1} / {topic.Levels.Count}");
                ImGui.PopFont();

                ImGui.PushFont(Fonts.FontNormal);
                CenteredText(topic.Title);
                ImGui.PopFont();
            }
            ImGui.EndChild();

            ImGui.SameLine(0, 4);

            ImGui.BeginChild("Right", new Vector2(0, 0), false, ImGuiWindowFlags.NoBackground);
            {
                FormInputs.AddVerticalSpace(30);
                ImGui.Indent(10 * T3Ui.UiScaleFactor);

                // COMPLETED section
                CustomComponents.StylizedText("COMPLETED", Fonts.FontSmall, UiColors.Text.Fade(0.3f));
                CustomComponents.StylizedText(previousLevel.Title, Fonts.FontLarge, UiColors.Text.Fade(0.3f));

                FormInputs.AddVerticalSpace();
                ImGui.Separator();

                FormInputs.AddVerticalSpace();
                CustomComponents.StylizedText("NEXT UP", Fonts.FontSmall, UiColors.Text.Fade(0.3f));

                ImGui.PushFont(Fonts.FontLarge);
                ImGui.TextWrapped(nextLevel.Title);
                ImGui.PopFont();
            }
            ImGui.EndChild();
        }
        ImGui.EndChild();
    }

    private static void DrawActionBar()
    {
        var style = ImGui.GetStyle();
        var btnH = ImGui.GetFrameHeight();
        //var wBack = ImGui.CalcTextSize("Back to Hub").X + style.FramePadding.X * 2;
        var wSkip = ImGui.CalcTextSize("Skip").X + style.FramePadding.X * 2;
        var wCont = ImGui.CalcTextSize("Continue").X + style.FramePadding.X * 2;
        var totalW = wSkip + wCont + style.ItemSpacing.X * 2;

        ImGui.PushStyleColor(ImGuiCol.Button, Color.Transparent.Rgba);
        if (ImGui.Button("Back to Hub", Vector2.Zero))
        {
            SkillManager.SaveResult(SkillProgression.LevelResult.States.Skipped);
            SkillManager.ExitPlayMode();
        }

        var right = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X;
        ImGui.SetCursorPosX(right - totalW);

        ImGui.SameLine(ImGui.GetWindowWidth() - totalW);
        if (ImGui.Button("Skip", new Vector2(wSkip, btnH)))
        {
            //SkillManager.CompleteAndProgressToNextLevel(SkillProgression.LevelResult.States.Skipped);
            SkillManager.SaveResult(SkillProgression.LevelResult.States.Skipped);
            SkillManager.UpdateActiveTopicAndLevel();
        }

        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.20f, 0.45f, 0.95f, 1f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.25f, 0.55f, 1.00f, 1f));
        if (ImGui.Button("Continue", new Vector2(wCont, btnH)))
        {
            SkillManager.CompleteAndProgressToNextLevel(SkillProgression.LevelResult.States.Completed);
        }

        ImGui.PopStyleColor(2);
    }

    private static void CenteredText(string text)
    {
        var labelSize = ImGui.CalcTextSize(text);
        var availableSize = ImGui.GetWindowSize().X;
        ImGui.SetCursorPosX(availableSize / 2 - labelSize.X / 2);
        ImGui.TextUnformatted(text);
    }

    private static void DrawTorusProgress(ImDrawListPtr dl, Vector2 center, float radius, float progress, Color color)
    {
        dl.PathClear();
        var opening = 0.5f;

        var aMin = 0.5f * MathF.PI + opening;
        var aMax = 2.5f * MathF.PI - opening;
        dl.PathArcTo(center, radius, aMin, MathUtils.Lerp(aMin, aMax, progress), 64);
        dl.PathStroke(color, ImDrawFlags.None, 6);
    }

    internal static void Show()
    {
        ImGui.OpenPopup(ProgressionPopupId);
    }

    private const string ProgressionPopupId = "ProgressionPopup";
}