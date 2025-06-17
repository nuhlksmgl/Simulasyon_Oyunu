using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class InGameMarket : MonoBehaviour
{
    public static InGameMarket Instance { get; private set; }

    [Header("Veri Listeleri")]
    public List<Category> productCategories;

    [Header("UI Referansları")]
    public Transform categoryListParent;
    public GameObject categoryButtonPrefab;
    public Transform productGridParent;
    public GameObject productCardPrefab;

    [Header("Lisans Paneli UI")]
    public GameObject licensePurchasePanel;
    public GameObject productGridPanel;
    public TextMeshProUGUI licenseCategoryNameText;
    public TextMeshProUGUI licenseCostText;
    public Button buyLicenseButton;

    [Header("Diğer Referanslar")]
    public PlayerBalance playerBalance;

    [Header("Dükkan Genişletme Referansları")]
    public GameObject duvar1;
    public GameObject duvar2;

    private Category currentSelectedCategory;
    private MarketProduct shopExpansionProduct;
    private MarketProduct shelfProduct;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); }
        else { Instance = this; }

        FindSpecialProducts();
    }

    void Start()
    {
        InitializeMarket();
    }

    void FindSpecialProducts()
    {
        if (productCategories == null) return;
        foreach (var category in productCategories)
        {
            if (category.productsInCategory == null) continue;

            var expansion = category.productsInCategory.FirstOrDefault(p => p.productName == "Dükkan Genişletme");
            if (expansion != null) shopExpansionProduct = expansion;

            var shelf = category.productsInCategory.FirstOrDefault(p => p.productName == "Raf");
            if (shelf != null) shelfProduct = shelf;
        }
    }

    public void InitializeMarket()
    {
        if (productCategories != null && productCategories.Count > 0 && productCategories[0] != null)
        {
            productCategories[0].isUnlocked = true;
        }

        if (categoryListParent != null)
        {
            foreach (Transform child in categoryListParent) { Destroy(child.gameObject); }
        }

        if (productCategories != null && categoryButtonPrefab != null)
        {
            foreach (var category in productCategories)
            {
                GameObject buttonObj = Instantiate(categoryButtonPrefab, categoryListParent);
                buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = category.categoryName;
                buttonObj.GetComponent<Button>().onClick.AddListener(() => OnCategoryButtonClicked(category));
            }
        }

        Category defaultCategory = productCategories?.FirstOrDefault(c => c.isUnlocked);
        if (defaultCategory != null) OnCategoryButtonClicked(defaultCategory);
    }

    public void OnCategoryButtonClicked(Category selectedCategory)
    {
        currentSelectedCategory = selectedCategory;
        if (selectedCategory.isUnlocked)
        {
            productGridPanel.SetActive(true);
            licensePurchasePanel.SetActive(false);
            PopulateProductGrid(selectedCategory);
        }
        else
        {
            productGridPanel.SetActive(false);
            licensePurchasePanel.SetActive(true);
            licenseCategoryNameText.text = $"{selectedCategory.categoryName} Lisansı";
            licenseCostText.text = $"Ücret: {selectedCategory.categoryLicenseCost}$";
            buyLicenseButton.onClick.RemoveAllListeners();
            buyLicenseButton.onClick.AddListener(() => BuyCategoryLicense(selectedCategory));
        }
    }

    void PopulateProductGrid(Category category)
    {
        if (productGridParent == null || productCardPrefab == null) return;

        foreach (Transform child in productGridParent) { Destroy(child.gameObject); }

        if (category == null || category.productsInCategory == null) return;

        // KONTROL MESAJI: Bu log, döngüye kaç ürün girdiğini size söyleyecektir.
        Debug.Log($"'{category.categoryName}' kategorisi için {category.productsInCategory.Count} adet kart oluşturuluyor.");

        foreach (var product in category.productsInCategory)
        {
            GameObject cardInstance = Instantiate(productCardPrefab, productGridParent);
            cardInstance.GetComponent<ProductCardUI>().Setup(product, category, this);
        }

        // Layout'u yeniden hesaplamaya zorla (üst üste binmeyi engeller)
        Canvas.ForceUpdateCanvases();
        if (productGridParent is RectTransform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(productGridParent as RectTransform);
        }
    }

    public void BuyCategoryLicense(Category categoryToUnlock)
    {
        if (PlayerBalance.Instance.DeductBalance(categoryToUnlock.categoryLicenseCost))
        {
            categoryToUnlock.isUnlocked = true;
            OnCategoryButtonClicked(categoryToUnlock);
        }
    }

    public void ApplyInstantPurchase(MarketProduct product)
    {
        if (product.productName == "Dükkan Genişletme")
        {
            if (duvar1 != null) duvar1.SetActive(false);
            if (duvar2 != null) duvar2.SetActive(false);
            product.isPurchased = true;
        }
    }

    public bool CanPurchaseShelf()
    {
        if (shelfProduct == null || shopExpansionProduct == null) return false;
        if (!shopExpansionProduct.isPurchased)
            return shelfProduct.purchaseCount < 1;
        else
            return shelfProduct.purchaseCount < 7;
    }

    public List<MarketProduct> GetAllUnlockedProducts()
    {
        var allProducts = new List<MarketProduct>();
        if (productCategories == null) return allProducts;
        foreach (Category category in productCategories)
        {
            if (category.isUnlocked)
            {
                allProducts.AddRange(category.productsInCategory);
            }
        }
        return allProducts;
    }
}