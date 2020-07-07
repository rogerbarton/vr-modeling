#pragma once
#include <PluginAPI/IUnityInterface.h>

/**
 * Function pointer to a C# delegate: <code>void MyFct(string message)</code><p>
 * @note C# delegates are <code>fixed</code> by default, so we do not have to worry about these pointers becoming invalid
 * due to the Garbage Collector.
 */
typedef void(UNITY_INTERFACE_API* StringCallback)(const char* message);

/**
 * Print to the Unity Debug.Log. Check that the function pointer is not null before using
 * <example><code>if (DebugLog) DebugLog("Hello");</code></example>
 * This is what the LOG macro does, use that instead.
 * @see Callbacks like this are set in Initialize and reset to <c>nullptr</c> in UnityPluginUnload
 */
extern StringCallback DebugLog;
extern StringCallback DebugLogWarning;
extern StringCallback DebugLogError;

/**
 * Macro to easily concatenate strings using stringstream, use the operator&lt;&lt;<br/>
 * <example><code>STR("My value: " &lt;&lt; 123)</code></example>
 */
#define STR(message) static_cast<std::ostringstream &&>((std::ostringstream() << message)).str().data()
#ifndef NDEBUG
/**
 * Macro to easily and safely print to the Unity Debug.Log, disabled in release. Uses STR.<br/>
 * <example><code> LOG("My value: " &lt;&lt; 123)</code></example>
 */
#define LOG(message) if(DebugLog) { DebugLog(STR(message)); }
/**
 * Call Unity Debug.LogWarning safely. Uses STR.
 */
#define LOGWARN(message) if(DebugLogWarning) { DebugLogWarning(STR(message)); }
/**
 * Call Unity Debug.LogError safely. Uses STR.
 */
#define LOGERR(message) if(DebugLogError) { DebugLogError(STR(message)); }
#else
#define LOG(m)
#define LOGWARN(m)
#define LOGERR(m)
#endif
