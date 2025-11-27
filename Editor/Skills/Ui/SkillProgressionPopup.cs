using System.Diagnostics;
using ImGuiNET;
using T3.Core.Utils;
using T3.Editor.Gui;
using T3.Editor.Gui.Input;
using T3.Editor.Gui.Styling;
using T3.Editor.Gui.UiHelpers;
using T3.Editor.Skills.Data;
using T3.Editor.Skills.Training;
using Color = T3.Core.DataTypes.Vector.Color;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace T3.Editor.Skills.Ui;

/// <summary>
/// A dialog that is shown after level completion.
/// </summary>
internal static class SkillProgressionPopup
{
    internal static void Show()
    {
        ImGui.OpenPopup(ProgressionPopupId);
        StarShowerEffect.Reset();
        
        if (!SkillTraining.TryGetActiveTopicAndLevel(out var topic, out _))
            return;

        _topicSelection.Clear();
        _topicSelection.Add(topic);
        _mapCanvas.FocusToActiveTopics(_topicSelection);
    }    
    
    internal static void Draw()
    {
        var popUpSize = new Vector2(700, 260) * T3Ui.UiScaleFactor;

        // Center the popup in the main viewport
        var vp = ImGui.GetMainViewport();
        var pos = vp.Pos + (vp.Size - popUpSize) * 0.5f;
        ImGui.SetNextWindowSize(popUpSize, ImGuiCond.Always);
        ImGui.SetNextWindowPos(pos);

        if (!SkillTraining.TryGetActiveTopicAndLevel(out var topic, out var activeLevel))
            return;

        bool keepOpen = true;

        //if (ImGui.BeginPopupModal("ProgressionPopup", ref keepOpen))
        
        ImGui.PushStyleColor(ImGuiCol.PopupBg, UiColors.BackgroundFull.Rgba);
        if (ImGui.BeginPopup("ProgressionPopup", ImGuiWindowFlags.NoResize |
                                                                  ImGuiWindowFlags.NoMove))
        {
            var index = topic.Levels.IndexOf(activeLevel);
            Debug.Assert(index >= 0);

            if (index < 1 )
            {
                CustomComponents.EmptyWindowMessage("Can't find level...");
            }
            else
            {
                var previousLevel = topic.Levels[index - 1];
                
                if (index == topic.Levels.Count - 1)
                {
                    DrawTopicCompletedContent(topic, previousLevel, activeLevel,  index);
                }
                else if( index < topic.Levels.Count - 1)
                {
                    DrawNextLevelContent(topic,previousLevel, activeLevel,  index);
                }
                else
                {
                    CustomComponents.EmptyWindowMessage("Can't find level...");
                }
            }

            DrawActions();

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                StarShowerEffect.Reset();
            }
            
            ImGui.EndPopup();
            StarShowerEffect.DrawAndUpdate();
        }

        ImGui.PopStyleColor();
    }

    
    private static void DrawNextLevelContent(QuestTopic topic, QuestLevel previousLevel, QuestLevel nextLevel, int index)
    {
        var uiScale = T3Ui.UiScaleFactor;
        var dl = ImGui.GetWindowDrawList();

        var leftWidth = 240 * uiScale;
        ImGui.BeginChild("UpperArea", new Vector2(0, -40 * uiScale), false, ImGuiWindowFlags.NoBackground);
        {
            var area = ImRect.RectWithSize(ImGui.GetWindowPos(), ImGui.GetWindowSize());
            area.Expand(-10);
            dl.AddRectFilled(area.Min + new Vector2(leftWidth,0), area.Max, UiColors.WindowBackground, 7 * uiScale);

            ImGui.BeginChild("Map", new Vector2(leftWidth, 0), false, ImGuiWindowFlags.NoBackground);
            {
                _topicSelection??= new();
                var itemHovered=_mapCanvas.DrawContent(null, out _, _topicSelection);
                if (!itemHovered && ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    //SkillMapPopup.Show();
                }                
            }
            ImGui.EndChild();
            ImGui.SameLine();
            
            ImGui.SameLine(0, 4);

            ImGui.BeginChild("Right", new Vector2(0, 0), false, ImGuiWindowFlags.NoBackground);
            {
                FormInputs.AddVerticalSpace(30);
                ImGui.Indent(20 * T3Ui.UiScaleFactor);

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

    
    private static void DrawTopicCompletedContent(QuestTopic topic, QuestLevel previousLevel, QuestLevel nextLevel, int index)
    {
        var uiScale = T3Ui.UiScaleFactor;
        var dl = ImGui.GetWindowDrawList();

        var leftWidth = 240 * uiScale;
        ImGui.BeginChild("UpperArea", new Vector2(0, -40 * uiScale), false, ImGuiWindowFlags.NoBackground);
        {
            var area = ImRect.RectWithSize(ImGui.GetWindowPos(), ImGui.GetWindowSize());
            area.Expand(-10);
            dl.AddRectFilled(area.Min + new Vector2(leftWidth,0), area.Max, UiColors.WindowBackground, 7 * uiScale);

            ImGui.BeginChild("Map", new Vector2(leftWidth, 0), false, ImGuiWindowFlags.NoBackground);
            {
                _topicSelection??= new();
                var itemHovered=_mapCanvas.DrawContent(null, out _, _topicSelection);
                if (!itemHovered && ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    //SkillMapPopup.Show();
                }                
            }
            ImGui.EndChild();
            ImGui.SameLine();
            
            ImGui.SameLine(0, 4);

            ImGui.BeginChild("Right", new Vector2(0, 0), false, ImGuiWindowFlags.NoBackground);
            {
                FormInputs.AddVerticalSpace(30);
                ImGui.Indent(20 * T3Ui.UiScaleFactor);

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
    
    
    private static void DrawActions()
    {
        var indent = 10;
        ImGui.Indent(indent);
        var style = ImGui.GetStyle();
        var btnH = ImGui.GetFrameHeight();
        var wSkip = ImGui.CalcTextSize("Skip").X + style.FramePadding.X * 2;
        var wCont = ImGui.CalcTextSize("Continue").X + style.FramePadding.X * 2;
        var totalW = wSkip + wCont + style.ItemSpacing.X * 2 + indent;

        ImGui.PushStyleColor(ImGuiCol.Button, Color.Transparent.Rgba);
        if (ImGui.Button("Back to Hub", Vector2.Zero))
        {
            SkillTraining.SaveNewResult(SkillProgress.LevelResult.States.Skipped);
            SkillTraining.ExitPlayMode();
        }

        var right = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X;
        ImGui.SetCursorPosX(right - totalW);

        ImGui.SameLine(ImGui.GetWindowWidth() - totalW);
        if (ImGui.Button("Skip", new Vector2(wSkip, btnH)))
        {
            //SkillManager.CompleteAndProgressToNextLevel(SkillProgression.LevelResult.States.Skipped);
            SkillTraining.SaveNewResult(SkillProgress.LevelResult.States.Skipped);
            SkillTraining.UpdateTopicStatesAndProgression();
        }

        ImGui.PopStyleColor();

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.20f, 0.45f, 0.95f, 1f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.25f, 0.55f, 1.00f, 1f));
        if (ImGui.Button("Continue", new Vector2(wCont, btnH)))
        {
            SkillTraining.CompleteAndProgressToNextLevel(SkillProgress.LevelResult.States.Completed);
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

    private static HashSet<QuestTopic>  _topicSelection = [];
    private const string ProgressionPopupId = "ProgressionPopup";
    private static readonly SkillMapCanvas _mapCanvas = new();
}

/// <summary>
/// A simple "celebration" effect that can be shown on events like "level completed".
/// </summary>
internal static class StarShowerEffect
{
    internal static void DrawAndUpdate()
    {
        var center = GetCenter();
        var dl = ImGui.GetForegroundDrawList();

        var progress = (float)(ImGui.GetTime() - _startTime)/3.0f;

        for (var index = 0; index < _positions.Length; index++)
        {
            // Update position
            var rand = MathUtils.Hash01((uint)index);
            var p = _positions[index];
            var dFromCenter = p - center;
            var l = dFromCenter.Length();
            var dNorm = Vector2.Normalize(dFromCenter);
            //var f = 50f / (progress + 5f) + 0.2f * rand;
            var f = progress + 0.1f * rand;
            p += f * dNorm * (30/ (2*f+1)) + new Vector2(0, 3f) * (f + 0.5f);

            _positions[index] = p;

            Icons.DrawIconAtScreenPosition(Icon.Star, p, new Vector2(Icons.FontSize * 4f), dl, Color.Orange.Fade((1- 1.5f*f).Clamp(0, 1)));
        }
    }

    internal static void Reset()
    {
        _startTime = ImGui.GetTime();
        float radius = 30;
        var center = GetCenter() + new Vector2(0, -10);
        for (var index = 0; index < _positions.Length; index++)
        {
            var f = (float)index / Count + 0.223f;
            _positions[index] = center + new Vector2(MathF.Sin(f * MathF.Tau),
                                                     MathF.Cos(f * MathF.Tau)) * radius;
        }
    }

    private static Vector2 GetCenter()
    {
        var vp = ImGui.GetMainViewport();
        var windowSize = vp.Size;
        var center = windowSize * 0.5f;
        return center + new Vector2(-220, -10) * T3Ui.UiScaleFactor;
    }

    private static readonly Vector2[] _positions = new Vector2[Count];

    private const int Count = 30;
    private static double _startTime;
}