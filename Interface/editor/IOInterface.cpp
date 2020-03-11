#include "IOInterface.h"
#include <Eigen/core>
#include <igl/readOFF.h>
#include <igl/per_vertex_normals.h>

std::string modelRoot = "";

extern "C" {
    void InitializeNative(const StringCallback debugCallback) {
        //Note: may be called several times if the debugCallback changes
        DebugLog = debugCallback;
        if (DebugLog) DebugLog("Initialized Native.");
    }

    using V_t = Eigen::Matrix<float, Eigen::Dynamic, 3, Eigen::RowMajor>;
    using F_t = Eigen::Matrix<unsigned int, Eigen::Dynamic, 3, Eigen::RowMajor>;
    void LoadOFF(const char* path, void*& VPtr, int& VSize, void*& NPtr, int& NSize, void*& FPtr, int& FSize) {
        auto* V = new V_t(); //Must use new as we delete in C#
        auto* N = new V_t();
        auto* F = new F_t();
        
        bool success = igl::readOFF(path, *V, *F, *N);
        
        V->array() *= 0.1f; //Scaling factor to make it match the Unity scale

        if (N->rows() == 0) //Calculate normals if they are not present
            igl::per_vertex_normals(*V, *F, *N);

        VSize = V->rows();
        FSize = F->rows();
        NSize = N->rows();
        VPtr = V->data();
        FPtr = F->data();
        NPtr = N->data();
        
        if (DebugLog) DebugLog((std::string("OFF Import ") + std::string((success ? "Successful" : "Unsuccessful"))).data());
    }
}
