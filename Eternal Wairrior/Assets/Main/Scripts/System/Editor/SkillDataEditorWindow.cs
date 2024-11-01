using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SkillDataEditorWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private bool showValidationResults = true;
    private ValidationResult currentValidation;

    private void OnGUI()
    {
        DrawToolbar();
        DrawMainContent();
        DrawValidationPanel();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Validate All", EditorStyles.toolbarButton))
        {
            ValidateAllSkills();
        }

        if (GUILayout.Button("Create Backup", EditorStyles.toolbarButton))
        {
            AutoBackupManager.CreateBackup();
        }

        showValidationResults = GUILayout.Toggle(
            showValidationResults,
            "Show Validation",
            EditorStyles.toolbarButton
        );

        EditorGUILayout.EndHorizontal();
    }

    private void DrawValidationPanel()
    {
        if (!showValidationResults || currentValidation == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);

        if (currentValidation.HasErrors)
        {
            EditorGUILayout.LabelField("Errors:", EditorStyles.boldLabel);
            foreach (var error in currentValidation.Errors)
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
            }
        }

        if (currentValidation.HasWarnings)
        {
            EditorGUILayout.LabelField("Warnings:", EditorStyles.boldLabel);
            foreach (var warning in currentValidation.Warnings)
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }
        }

        if (!currentValidation.HasErrors && !currentValidation.HasWarnings)
        {
            EditorGUILayout.HelpBox("No issues found", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
    }

    private void ValidateAllSkills()
    {
        var skillDataManager = FindObjectOfType<SkillDataManager>();
        if (skillDataManager == null) return;

        var allSkills = skillDataManager.GetAllSkillData();
        var validationResults = new List<ValidationResult>();

        foreach (var skill in allSkills)
        {
            var result = SkillDataValidator.ValidateSkillData(skill);
            if (result.HasErrors || result.HasWarnings)
            {
                validationResults.Add(result);
            }
        }

        // 결과 표시
        currentValidation = new ValidationResult();
        foreach (var result in validationResults)
        {
            currentValidation.Errors.AddRange(result.Errors);
            currentValidation.Warnings.AddRange(result.Warnings);
        }

        Repaint();
    }
}