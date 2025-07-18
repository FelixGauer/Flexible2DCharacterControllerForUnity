using System.Collections.Generic;
using UnityEngine;

public class AnimationController
{
    private readonly Animator _animator;
    private readonly Dictionary<string, AnimationClip> _clipCache;
    private readonly Dictionary<string, int> _parameterHashCache;

    
    // Флаг для проверки валидности контроллера
    public bool IsValid { get; private set; }
    
    // Константы для общих параметров
    private readonly int _speedHash = Animator.StringToHash("Speed");
    private readonly int _speedMultiplierHash = Animator.StringToHash("SpeedMultiplier");
    
    public AnimationController(Animator animator)
    {
        _animator = animator;
        _clipCache = new Dictionary<string, AnimationClip>();
        _parameterHashCache = new Dictionary<string, int>();
        
        // Проверяем валидность и инициализируем
        IsValid = ValidateAnimator();
        if (IsValid)
        {
            CacheAnimationClips();
        }
    }
    
    private bool ValidateAnimator()
    {
        if (_animator == null)
        {
            Debug.LogWarning("AnimationController: Animator is null!");
            return false;
        }
        
        if (_animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("AnimationController: RuntimeAnimatorController is null!");
            return false;
        }
        
        return true;
    }
    
    private void CacheAnimationClips()
    {
        if (!IsValid) return;
        
        foreach (var clip in _animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && !string.IsNullOrEmpty(clip.name))
            {
                if (!_clipCache.ContainsKey(clip.name))
                {
                    _clipCache[clip.name] = clip;
                }
            }
        }
    }
    
    private int GetParameterHash(string parameterName)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            Debug.LogWarning("AnimationController: Parameter name is null or empty!");
            return 0;
        }
        
        if (!_parameterHashCache.ContainsKey(parameterName))
        {
            _parameterHashCache[parameterName] = Animator.StringToHash(parameterName);
        }
        return _parameterHashCache[parameterName];
    }
    
    /// <summary>
    /// Воспроизводит анимацию с заданной скоростью на основе желаемой длительности
    /// </summary>
    /// <param name="animationName">Имя анимации</param>
    /// <param name="desiredDuration">Желаемая длительность анимации</param>
    /// <param name="speedParameterName">Имя параметра скорости в аниматоре</param>
    /// <param name="layer">Слой анимации</param>
    /// <param name="normalizedTime">Нормализованное время начала</param>
    public bool PlayAnimationWithDuration(string animationName, float desiredDuration, 
        string speedParameterName = "SpeedMultiplier", int layer = 0, float normalizedTime = 0f)
    {
        if (!IsValid) return false;
        
        if (string.IsNullOrEmpty(animationName))
        {
            Debug.LogWarning("AnimationController: Animation name is null or empty!");
            return false;
        }
        
        if (desiredDuration <= 0f)
        {
            Debug.LogWarning($"AnimationController: Invalid duration {desiredDuration} for animation '{animationName}'!");
            return false;
        }
        
        if (!_clipCache.TryGetValue(animationName, out var clip))
        {
            Debug.LogWarning($"AnimationController: Animation clip '{animationName}' not found!");
            return false;
        }
        
        if (clip.length <= 0f)
        {
            Debug.LogWarning($"AnimationController: Animation clip '{animationName}' has invalid length!");
            return false;
        }
        
        float speedMultiplier = clip.length / desiredDuration;
        int paramHash = GetParameterHash(speedParameterName);
        
        if (paramHash == 0) return false;
        
        _animator.SetFloat(paramHash, speedMultiplier);
        _animator.Play(animationName, layer, normalizedTime);
        
        return true;
    }
    
    /// <summary>
    /// Воспроизводит анимацию с обычной скоростью
    /// </summary>
    public bool PlayAnimation(string animationName, int layer = 0, float normalizedTime = 0f)
    {
        if (!IsValid) return false;
        
        if (string.IsNullOrEmpty(animationName))
        {
            Debug.LogWarning("AnimationController: Animation name is null or empty!");
            return false;
        }
        
        if (!HasClip(animationName))
        {
            Debug.LogWarning($"AnimationController: Animation clip '{animationName}' not found!");
            return false;
        }
        
        _animator.Play(animationName, layer, normalizedTime);
        return true;
    }
    
    /// <summary>
    /// Синхронизирует скорость анимации с физической скоростью движения
    /// </summary>
    /// <param name="currentSpeed">Текущая скорость движения</param>
    /// <param name="baseSpeed">Базовая скорость для нормализации</param>
    /// <param name="speedParameterName">Имя параметра скорости</param>
    public bool SyncAnimationWithMovement(float currentSpeed, float baseSpeed, 
        string speedParameterName = "Speed")
    {
        if (!IsValid) return false;
        
        if (baseSpeed <= 0f)
        {
            Debug.LogWarning("AnimationController: Base speed must be greater than 0!");
            return false;
        }
        
        if (currentSpeed < 0f)
        {
            Debug.LogWarning("AnimationController: Current speed cannot be negative!");
            return false;
        }
        
        float normalizedSpeed = currentSpeed / baseSpeed;
        return SetFloat(speedParameterName, normalizedSpeed);
    }
    
    /// <summary>
    /// Устанавливает параметр float в аниматоре
    /// </summary>
    public bool SetFloat(string parameterName, float value)
    {
        if (!IsValid) return false;
        
        int paramHash = GetParameterHash(parameterName);
        if (paramHash == 0) return false;
        
        // Проверяем, что параметр существует в аниматоре
        if (!HasParameter(parameterName, AnimatorControllerParameterType.Float))
        {
            Debug.LogWarning($"AnimationController: Float parameter '{parameterName}' not found in animator!");
            return false;
        }
        
        _animator.SetFloat(paramHash, value);
        return true;
    }
    
    /// <summary>
    /// Устанавливает параметр bool в аниматоре
    /// </summary>
    public bool SetBool(string parameterName, bool value)
    {
        if (!IsValid) return false;
        
        int paramHash = GetParameterHash(parameterName);
        if (paramHash == 0) return false;
        
        if (!HasParameter(parameterName, AnimatorControllerParameterType.Bool))
        {
            Debug.LogWarning($"AnimationController: Bool parameter '{parameterName}' not found in animator!");
            return false;
        }
        
        _animator.SetBool(paramHash, value);
        return true;
    }
    
    /// <summary>
    /// Устанавливает параметр trigger в аниматоре
    /// </summary>
    public bool SetTrigger(string parameterName)
    {
        if (!IsValid) return false;
        
        int paramHash = GetParameterHash(parameterName);
        if (paramHash == 0) return false;
        
        if (!HasParameter(parameterName, AnimatorControllerParameterType.Trigger))
        {
            Debug.LogWarning($"AnimationController: Trigger parameter '{parameterName}' not found in animator!");
            return false;
        }
        
        _animator.SetTrigger(paramHash);
        return true;
    }
    
    /// <summary>
    /// Проверяет наличие параметра в аниматоре
    /// </summary>
    private bool HasParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (!IsValid) return false;
        
        foreach (var param in _animator.parameters)
        {
            if (param.name == parameterName && param.type == parameterType)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Сбрасывает параметр скорости к базовому значению
    /// </summary>
    public bool ResetSpeedParameter(string speedParameterName = "SpeedMultiplier", float baseValue = 1f)
    {
        return SetFloat(speedParameterName, baseValue);
    }
    
    /// <summary>
    /// Получает информацию о текущем состоянии анимации
    /// </summary>
    public AnimatorStateInfo? GetCurrentAnimatorStateInfo(int layer = 0)
    {
        if (!IsValid) return null;
        
        return _animator.GetCurrentAnimatorStateInfo(layer);
    }
    
    /// <summary>
    /// Проверяет, завершена ли анимация
    /// </summary>
    public bool IsAnimationComplete(int layer = 0, float threshold = 0.95f)
    {
        if (!IsValid) return false;
        
        var stateInfo = _animator.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.normalizedTime >= threshold;
    }
    
    /// <summary>
    /// Получает длительность анимационного клипа
    /// </summary>
    public float GetClipLength(string animationName)
    {
        if (!IsValid) return 0f;
        
        if (string.IsNullOrEmpty(animationName))
        {
            Debug.LogWarning("AnimationController: Animation name is null or empty!");
            return 0f;
        }
        
        if (_clipCache.TryGetValue(animationName, out var clip))
        {
            return clip.length;
        }
        
        Debug.LogWarning($"AnimationController: Animation clip '{animationName}' not found!");
        return 0f;
    }
    
    /// <summary>
    /// Проверяет, есть ли анимационный клип с заданным именем
    /// </summary>
    public bool HasClip(string animationName)
    {
        if (!IsValid) return false;
        
        if (string.IsNullOrEmpty(animationName)) return false;
        
        return _clipCache.ContainsKey(animationName);
    }
    
    /// <summary>
    /// Получает список всех доступных анимационных клипов
    /// </summary>
    public IReadOnlyCollection<string> GetAvailableClips()
    {
        return _clipCache.Keys;
    }
    
    /// <summary>
    /// Безопасно уничтожает контроллер
    /// </summary>
    public void Dispose()
    {
        _clipCache?.Clear();
        _parameterHashCache?.Clear();
        IsValid = false;
    }
}