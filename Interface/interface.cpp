#include "interface.h"
#include <Eigen/core>
#include <igl/jet.h>


extern "C"{
    void InitializeNative(DebugCallback callback) {
        DebugLog = callback;
        Eigen::Vector2d v;
        v << 1, 1;
    }

    int IncrementValue(int value){
        if (DebugLog) DebugLog("Incrementing.");
        return ++value;
    }
}
