#include "interface.h"
#include <Eigen/core>
#include <igl/readOBJ.h>

std::string modelRoot = "";

extern "C"{
    void InitializeNative(char const* const modelRootp, StringCallback debugCallback, VFCallback createMeshCallback) {
        modelRoot = modelRootp;

        DebugLog = debugCallback;
        CreateMesh = createMeshCallback;

        //Testing
        LoadMesh("bumpy-cube.obj");
    }

    int IncrementValue(int value){
        if (DebugLog) DebugLog("Incrementing.");
        return ++value;
    }

    void LoadMesh(std::string modelPath) {
        Eigen::MatrixXf V;
        Eigen::MatrixXi F;
        igl::readOBJ(modelRoot.append(modelPath), V, F);

        DebugLog(modelRoot.append(modelPath).append(" mesh loading").data());

        CreateMesh(V.data(), F.data());
    }
}
