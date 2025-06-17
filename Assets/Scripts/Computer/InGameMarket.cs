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
        if (productCategories.Count > 0 && productCategories[0] != null)
        {
            productCategories[0].isUnlocked = true;
        }
        foreach (Transform child in categoryListParent) { Destroy(child.gameObject); }
        foreach (var category in productCategories)
        {
            GameObject buttonObj = Instantiate(categoryButtonPrefab, categoryListParent);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = category.categoryName;
            buttonObj.GetComponent<Button>().onClick.AddListener(() => OnCategoryButtonClicked(category));
        }
        Category defaultCategory = productCategories.FirstOrDefault(c => c.isUnlocked);
        if (defaultCategory != null)
        {
            OnCategoryButtonClicked(defaultCategory);
        }
        else
        {
            Debug.LogWarning("Başlangıçta kilidi açık hiçbir kategori bulunamadı!");
        }
    }

    public void OnCategoryButtonClicked(Category selectedCategory)
    {
        Debug.Log($"--- OnCategoryButtonClicked METODU ÇAĞRILDI: Kategori = {selectedCategory.categoryName} ---");

        currentSelectedCategory = selectedCategory;

        Debug.Log($"Kategorinin kilit durumu (isUnlocked): {selectedCategory.isUnlocked}");

        if (selectedCategory.isUnlocked)
        {
            Debug.Log("IF bloğuna girildi. Ürünler listelenecek...");
            productGridPanel.SetActive(true);
            licensePurchasePanel.SetActive(false);
            PopulateProductGrid(selectedCategory);
        }
        else
        {
            Debug.Log("ELSE bloğuna girildi. Lisans satın alma paneli gösterilecek.");
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
        // Hatanın düzeltildiği satır burası
        Debug.Log($"PopulateProductGrid içindeyim: '{category.categoryName}' kategorisindeki {category.productsInCategory.Count} ürün işlenecek.");
        foreach (Transform child in productGridParent)
        {
            Destroy(child.gameObject);
        }

        if (category == null || category.productsInCategory == null) return;

        foreach (var product in category.productsInCategory)
        {
            GameObject cardInstance = Instantiate(productCardPrefab, productGridParent);
            cardInstance.GetComponent<ProductCardUI>().Setup(product, category, this);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(productGridParent as RectTransform);
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