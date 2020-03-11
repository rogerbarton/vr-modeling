#include "IOInterface.h"
#include <Eigen/core>
#include <igl/readOBJ.h>
#include <igl/readOFF.h>
#include <igl/jet.h>

std::string modelRoot = "";

extern "C" {
    void InitializeNative(const StringCallback debugCallback) {
        //Note: may be called several times if the debugCallback changes
        DebugLog = debugCallback;
        if (DebugLog) DebugLog("Initialized Native.");
    }

    void LoadOFF(const char* path, void*& VPtr, int& VSize, void*& NPtr, int NSize, void*& FPtr, int& FSize) {
        auto* V = new Eigen::MatrixXf(); //Must use new as we delete in C#
        auto* F = new Eigen::MatrixXi();
        auto* N = new Eigen::MatrixXf();
        
        bool success = igl::readOFF(path, *V, *F, *N);
        
        VSize = V->rows();
        FSize = F->rows();
        NSize = N->rows();
        VPtr = V->data();
        FPtr = F->data();
        NPtr = N->data();
        
        if (DebugLog) DebugLog((std::string("OFF Import ") + std::string((success ? "Successful" : "Unsuccessful"))).data());
    }
}
