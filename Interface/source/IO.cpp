#include "Interface.h"
#include "InterfaceTypes.h"
#include "NumericTypes.h"
#include <Eigen/core>
#include <igl/readOFF.h>
#include <igl/per_vertex_normals.h>

extern "C" {
    void TransposeInPlace(void* MatrixPtr, int rows, int cols) {
        auto V = Eigen::Map<Eigen::MatrixXf>((float*)MatrixPtr, rows, cols);
        //TODO: This will not work on a Eigen::Map as the dimensions are fixed, only for square matrices.
        //Reason Eigen::Map does not own the memory. See https://gitlab.com/libeigen/eigen/-/issues/749
        V.transposeInPlace();
    }

    void TransposeTo(void* InMatrixPtr, void* OutMatrixPtr, int rows, int cols) {
        auto In = Eigen::Map<Eigen::MatrixXf>((float*)InMatrixPtr, rows, cols);
        auto Out = Eigen::Map<Eigen::MatrixXf>((float*)OutMatrixPtr, cols, rows);
        Out = In.transpose();
    }

    void LoadOFF(const char* path, const float scale, void*& VPtr, int& VSize, void*& NPtr, int& NSize, void*& FPtr, int& FSize) {
        auto* V = new V_RowMajor_t(); //Must use new as we delete in C#
        auto* N = new V_RowMajor_t();
        auto* F = new F_RowMajor_t();
        
        bool success = igl::readOFF(path, *V, *F, *N);
        
        V->array() *= scale; //Scaling factor to make it match the Unity scale

        //if (N->rows() == 0) //Calculate normals if they are not present
            //igl::per_vertex_normals(*V, *F, *N);

        VSize = V->rows();
        FSize = F->rows();
        NSize = N->rows();
        VPtr = V->data();
        FPtr = F->data();
        NPtr = N->data();
        
        if (DebugLog) DebugLog((std::string("OFF Import ") + std::string((success ? "Successful: " : "Unsuccessful: ")) + std::string(path)).data());
    }
}
