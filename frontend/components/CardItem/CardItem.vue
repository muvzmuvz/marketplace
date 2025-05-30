<script setup>
defineProps({
  product: Object, // Принимаем объект product
});

import '@/assets/css/main.css';
</script>

<template>
  <div class="card-items">
    <img :src="product.images[0].path" :alt="product.name" class="card-img" />
    <p class="card--descripion">{{ product.name }}</p>
    <p class="card-coast">Цена: {{ product.price.toLocaleString() }} ₽</p>
    <p class="card--ps text-gray-500">Осталось: {{ product.countProduct }} шт.</p>
    <Button class="order--buton" @click="addToCard">Заказать</Button>
  </div>
</template>