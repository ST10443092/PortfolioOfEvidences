package com.example.prog7313p1

data class Category(
    val id: String = "",
    val userId: String = "",
    val name: String = "",
    val description: String? = null,
    val type: String = ""

) {
    override fun toString(): String {
        return name
    }
}
