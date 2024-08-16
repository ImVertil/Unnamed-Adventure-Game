using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Test Calculator", menuName = "Effect/New Test Calculator")]
public class TestExecutionCalculator : ExecutionCalculator
{
    protected override float GetCalculatedValue()
    {
        return base.GetCalculatedValue();
    }
}