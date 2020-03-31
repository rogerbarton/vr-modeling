#include "Interface.h"
#include "InterfaceTypes.h"
#include "NumericTypes.h"
#include <Eigen/core>
#include <igl/readOFF.h>
#include <igl/per_vertex_normals.h>

extern "C" {
    void ToColMajor(void* MatrixPtr, int cols) {
        Eigen::MatrixXf V = Eigen::Map<Eigen::MatrixXf>((float*)MatrixPtr, cols, 3);
        V.transposeInPlace();
    }

    void LoadOFF(const char* path, const float scale, void*& VPtr, int& VSize, void*& NPtr, int& NSize, void*& FPtr, int& FSize) {
        auto* V = new V_t(); //Must use new as we delete in C#
        auto* N = new V_t();
        auto* F = new F_t();
        
        bool success = igl::readOFF(path, *V, *F, *N);
        
        V->array() *= scale; //Scaling factor to make it match the Unity scale

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
